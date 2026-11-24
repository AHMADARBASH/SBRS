using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.BackgroundJobs;

/// <summary>
/// Periodically deletes refresh tokens that are past their expiry.
/// Used and revoked tokens are deliberately kept until they expire: reuse detection must still
/// find a burned token that is presented again before its expiry, otherwise a replay would look
/// like an unknown token instead of a reused one.
/// </summary>
public sealed class RefreshTokenCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<RefreshTokenCleanupSettings> settings,
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    /// <summary>The first run waits briefly so startup work (migrations, seeders) finishes first.</summary>
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);

    private const int DefaultIntervalHours = 24;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalHours = settings.Value.IntervalHours;
        if (intervalHours <= 0)
        {
            // A bad configuration value must not stop the host (PeriodicTimer rejects it).
            logger.LogWarning(
                "RefreshTokenCleanup:IntervalHours must be positive; falling back to {Hours} hours.",
                DefaultIntervalHours);
            intervalHours = DefaultIntervalHours;
        }

        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return; // the host stopped before the first run
        }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(intervalHours));

        do
        {
            await RunCleanupAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        try
        {
            // AppDbContext is scoped, so every run gets a fresh scope and connection.
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // One set-based DELETE, nothing is loaded into memory. ExpiresAt is nullable
            // ("never expires"); NULL < now() is never true, so those rows are never touched.
            var deleted = await db.RefreshTokens
                .Where(token => token.ExpiresAt < DateTime.UtcNow)
                .ExecuteDeleteAsync(ct);

            logger.LogInformation(
                "Refresh token cleanup deleted {DeletedCount} expired token(s).", deleted);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // host shutdown: stop the worker, do not report it as a failure
        }
        catch (Exception ex)
        {
            // A failed run must not kill the worker; the next tick retries.
            logger.LogError(ex, "Refresh token cleanup failed.");
        }
    }
}
