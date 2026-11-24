namespace SBRS.Infrastructure.BackgroundJobs;

/// <summary>
/// Configuration for the periodic refresh-token cleanup, bound from the
/// "RefreshTokenCleanup" configuration section.
/// </summary>
public sealed class RefreshTokenCleanupSettings
{
    public const string SectionName = "RefreshTokenCleanup";

    /// <summary>Hours between two cleanup runs. Default: once per day.</summary>
    public int IntervalHours { get; set; } = 24;
}
