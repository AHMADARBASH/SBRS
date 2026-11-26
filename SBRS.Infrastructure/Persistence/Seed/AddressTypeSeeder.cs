using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Persistence.Seed;

/// <summary>Inserts only address types whose English name is missing; existing rows are unchanged.</summary>
public class AddressTypeSeeder(AppDbContext db, ILogger<AddressTypeSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existing = (await db.AddressTypes.Select(t => t.EnName).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        foreach (var (arName, enName) in AddressTypeSeed.Rows)
        {
            if (existing.Contains(enName.Trim()))
                continue;

            db.AddressTypes.Add(new AddressTypeEntity(arName.Trim(), enName.Trim()));
            existing.Add(enName.Trim());
            inserted++;
        }

        if (inserted > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation("Reference data {Entity}: {Inserted} inserted of {Total} defined rows.",
            nameof(AddressTypeEntity), inserted, AddressTypeSeed.Rows.Count);
    }
}

public static class AddressTypeSeed
{
    public static readonly IReadOnlyList<(string ArName, string EnName)> Rows =
    [
        ("منشأة صحية", "Health facility"),
        ("مسجد", "Mosque"),
        ("مدرسة", "School"),
        ("منشأة مدنية محلية", "Local Facility"),
        ("ملجأ", "Shelter"),
        ("شارع", "Street"),
    ];
}
