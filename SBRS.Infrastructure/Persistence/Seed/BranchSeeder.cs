using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Identity;

namespace SBRS.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent SARC branches: existing rows are matched by (organizationId, code) first and
/// then by English name, so a branch created manually is corrected instead of duplicated.
/// Nothing is ever deleted, and the HQ branch is left alone unless it is one of the rows below.
/// </summary>
public class BranchSeeder(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<BranchSeeder> logger)
{
    private const string SarcOrganizationCode = OrganizationSeed.Code;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var organizationId = await ResolveOrganizationIdAsync(ct);
        if (organizationId is null)
            return;

        var createdBy = await ResolveSystemAdminIdAsync();
        if (createdBy is null)
            logger.LogWarning("Branch seed: no SystemAdmin user found; branches will be created without CreatedBy.");

        var existing = await db.Branches.ToListAsync(ct);
        var inserted = 0;

        foreach (var (enName, arName, rawCode) in BranchSeed.Rows)
        {
            var code = rawCode.Trim().ToUpperInvariant();

            var branch = existing.FirstOrDefault(b => b.organizationId == organizationId && b.Code == code)
                         ?? existing.FirstOrDefault(b =>
                             string.Equals(b.EnName, enName, StringComparison.OrdinalIgnoreCase));

            if (branch is not null)
            {
                if (branch.Code != code || branch.ArName != arName)
                    branch.ApplySeed(arName, code);

                continue;
            }

            var newBranch = new BranchEntity(organizationId.Value, arName, enName, code, true);
            db.Branches.Add(newBranch);

            if (createdBy is { } userId)
                db.Entry(newBranch).Property(b => b.CreatedBy).CurrentValue = userId;

            inserted++;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Branch seed: {Inserted} inserted of {Total} defined rows.",
            inserted, BranchSeed.Rows.Count);
    }

    /// <summary>
    /// The branches belong to the SARC organization, identified by its system-wide code.
    /// As a safe fallback a database with exactly one organization uses that one; anything
    /// else is skipped with a warning instead of guessing.
    /// </summary>
    private async Task<Guid?> ResolveOrganizationIdAsync(CancellationToken ct)
    {
        var organizations = await db.Organizations.AsNoTracking().ToListAsync(ct);

        var sarc = organizations.FirstOrDefault(o =>
            string.Equals(o.Code, SarcOrganizationCode, StringComparison.OrdinalIgnoreCase));

        if (sarc is not null)
            return sarc.Id;

        if (organizations.Count == 1)
            return organizations[0].Id;

        logger.LogWarning(
            "Branch seed skipped: organization '{Code}' was not found and there is not exactly one organization ({Count}).",
            SarcOrganizationCode, organizations.Count);
        return null;
    }

    /// <summary>Uses the same SystemAdmin account the identity seeder creates from configuration.</summary>
    private async Task<Guid?> ResolveSystemAdminIdAsync()
    {
        var configuredUserName = configuration["SeedAdmin:UserName"];
        if (!string.IsNullOrWhiteSpace(configuredUserName))
        {
            var configured = await userManager.FindByNameAsync(configuredUserName);
            if (configured is not null && await userManager.IsInRoleAsync(configured, Roles.SystemAdmin))
                return configured.Id;
        }

        var systemAdmins = await userManager.GetUsersInRoleAsync(Roles.SystemAdmin);
        return systemAdmins.OrderBy(u => u.CreatedAt).FirstOrDefault()?.Id;
    }
}

/// <summary>The SARC branches exactly as specified; the code is the stable key used by facility codes.</summary>
public static class BranchSeed
{
    public static readonly IReadOnlyList<(string EnName, string ArName, string Code)> Rows =
    [
        ("Hama branch", "فرع حماة", "HAM"),
        ("Lattakia branch", "فرع اللاذقية", "LAT"),
        ("Idleb branch", "فرع إدلب", "IDL"),
        ("Al-Hasakeh branch", "فرع الحسكة", "HAS"),
        ("Deir-ez-Zor branch", "فرع دير الزور", "DRZ"),
        ("Tartous branch", "فرع طرطوس", "TAR"),
        ("Ar-Raqqa branch", "فرع الرقة", "RAQ"),
        ("Dar'a branch", "فرع درعا", "DAR"),
        ("As-Sweida branch", "فرع السويداء", "SWE"),
        ("Quneitra branch", "فرع القنيطرة", "QUN"),
        ("Damascus branch", "فرع دمشق", "DAM"),
        ("Aleppo branch", "فرع حلب", "ALP"),
        ("Rural Damascus branch", "فرع ريف دمشق", "RUD"),
        ("Homs branch", "فرع حمص", "HMS"),
    ];
}
