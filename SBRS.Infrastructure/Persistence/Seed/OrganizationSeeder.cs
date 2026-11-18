using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent SARC organization: a fresh database has no organization at all, and the branch
/// seeder deliberately refuses to guess one, so the branches would silently never be created.
/// Matched by code only — an organization created by hand keeps its names and nothing is duplicated.
/// </summary>
public class OrganizationSeeder(AppDbContext db, ILogger<OrganizationSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        // read in memory: the code comparison must not depend on the column collation
        var existing = (await db.Organizations.ToListAsync(ct))
            .FirstOrDefault(o => string.Equals(o.Code, OrganizationSeed.Code, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            logger.LogInformation("Organization seed: '{Code}' already exists.", OrganizationSeed.Code);
            return;
        }

        db.Organizations.Add(new OrganizationEntity(
            OrganizationSeed.ArName, OrganizationSeed.EnName, OrganizationSeed.Code));

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Organization seed: '{Code}' created.", OrganizationSeed.Code);
    }
}

/// <summary>The owning organization every seeded branch belongs to; the code is the stable key.</summary>
public static class OrganizationSeed
{
    public const string Code = "SARC";
    public const string ArName = "الهلال الأحمر العربي السوري";
    public const string EnName = "Syrian Arab Red Crescent";
}
