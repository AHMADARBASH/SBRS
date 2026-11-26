using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Persistence.Seed;

/// <summary>
/// Identity types from the local seed_identity_types.sql asset, now part of the system.
/// Inserts only codes that are missing; existing rows (including their order) are never changed.
/// </summary>
public class IdentityTypeSeeder(AppDbContext db, ILogger<IdentityTypeSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existing = (await db.IdentityTypes.Select(i => i.Code).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        foreach (var row in IdentityTypeSeed.Rows)
        {
            if (existing.Contains(row.Code))
                continue;

            var entity = new IdentityTypeEntity(
                row.ArName.Trim(), row.EnName.Trim(), row.Code.Trim(),
                row.ValueRegex.Trim(), row.DisplayIndex)
            {
                Id = Guid.Parse(row.Id)
            };

            db.IdentityTypes.Add(entity);
            existing.Add(row.Code);
            inserted++;
        }

        if (inserted > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation("Reference data {Entity}: {Inserted} inserted of {Total} defined rows.",
            nameof(IdentityTypeEntity), inserted, IdentityTypeSeed.Rows.Count);
    }
}

public sealed record IdentityTypeSeedRow(
    string Id, string Code, string ArName, string EnName, string ValueRegex, int DisplayIndex);

public static class IdentityTypeSeed
{
    public static readonly IReadOnlyList<IdentityTypeSeedRow> Rows =
    [
        new("dbeade04-9647-4c6e-b8f9-23adb8c48684", "FAMILY_BOOK", "دفتر عائلة", "Family Book",
            @"^[\p{L}\p{N}/-]{1,30}$", 2),
        new("e06af3b8-f67a-49a1-b28b-263be8f85a04", "VACCINATION_CARD", "بطاقة لقاحات",
            "Vaccination Card", @"^[\p{L}\p{N}/-]{1,30}$", 4),
        new("2eae470c-bc84-42d4-ba8c-2c82d4f99314", "PASSPORT", "جواز سفر", "Passport",
            @"^[A-Za-z0-9/-]{1,20}$", 3),
        new("e565ad7a-722c-4c80-a6b8-42cdc65ca458", "NATIONAL_ID", "هوية وطنية", "National ID",
            @"^\d{10}$", 1),
        new("62c95ac8-0f26-471d-8db3-5be6e8da471d", "CIVIL_REGISTRY", "قيد نفوس", "Civil Registry",
            @"^[\p{L}\p{N}/-]{1,30}$", 5),
        new("2448400a-abdd-463d-8609-6c07bb2b6ea3", "UNREGISTERED", "مكتوم القيد", "Unregistered",
            @"^$", 6),
        new("b70119d6-1581-4ac5-b7cd-8a6d181ff064", "DRIVING_LICENSE", "شهادة قيادة",
            "Driving License", @"^[\p{L}\p{N}/-]{1,30}$", 7),
        new("b3c24df7-3802-4a58-b95e-a041c3a22c97", "BIRTH_CERTIFICATE", "شهادة ميلاد",
            "Birth Certificate", @"^[\p{L}\p{N}/-]{1,30}$", 8),
        new("5a9964a8-9e5e-485b-a52c-be4b3e3e924c", "DISABILITY_CERTIFICATE", "شهادة إعاقة",
            "Disability Certificate", @"^[\p{L}\p{N}/-]{1,30}$", 9),
        new("ed3322b0-2ec9-4633-af4d-da04b5031710", "FAMILY_STATEMENT", "بيان عائلي",
            "Family Statement", @"^[\p{L}\p{N}/-]{1,30}$", 10),
        new("42997f79-56e4-43c4-82ab-ed1b51928306", "MARRIAGE_CERTIFICATE", "وثيقة زواج",
            "Marriage Certificate", @"^[\p{L}\p{N}/-]{1,30}$", 11),
        new("9f2e7c6a-58d1-47f9-b4e1-2b8f0d54c3a2", "NO_ID", "لا توجد وثيقة", "No Document",
            @"^$", 12),
    ];
}
