using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Persistence.Seed;

/// <summary>Inserts only the codes that are missing; existing rows are never changed.</summary>
public class ClinicTypeSeeder(AppDbContext db, ILogger<ClinicTypeSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existing = (await db.ClinicTypes.Select(t => t.Code).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        foreach (var (code, arName, enName) in ClinicTypeSeed.Rows)
        {
            if (existing.Contains(code))
                continue;

            db.ClinicTypes.Add(new ClinicTypeEntity(arName, enName, code));
            inserted++;
        }

        if (inserted > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation("Reference data {Entity}: {Inserted} inserted of {Total} defined rows.",
            nameof(ClinicTypeEntity), inserted, ClinicTypeSeed.Rows.Count);
    }
}

public static class ClinicTypeSeed
{
    public static readonly IReadOnlyList<(string Code, string ArName, string EnName)> Rows =
    [
        ("PHYSIOTHERAPY", "العلاج الفيزيائي", "Physiotherapy"),
        ("ENT", "أذن انف حنجرة", "ENT"),
        ("DENTAL", "أسنان", "Dental"),
        ("PEDIATRIC", "أطفال", "Pediatric"),
        ("UROLOGY", "البولية", "Urology"),
        ("NUTRITION", "التغذية", "Nutrition"),
        ("DERMATOLOGY", "الجلدية", "Dermatology"),
        ("INTERNAL_MEDICINE", "الداخلية", "Internal Medicine"),
        ("NEUROLOGICAL", "العصبية", "Neurological"),
        ("ORTHOPEDIC", "العظمية", "Orthopedic"),
        ("OPHTHALMOLOGY", "العيادة العينية", "Ophthalmology"),
        ("MENTAL_HEALTH", "العيادة النفسية", "Mental Health Clinic"),
        ("ENDOCRINOLOGICAL", "الغدد", "Endocrinological"),
        ("NEPHROLOGY", "الكلية", "Nephrology"),
        ("PSYCHOEDUCATION", "المشورة النفسية", "Psycho education"),
        ("GYNECOLOGY", "النسائية", "Gynecology"),
        ("DIGESTIVE", "الهضمية", "Digestive clinic"),
        ("CARDIOLOGY", "قلبية", "Cardiology"),
    ];
}
