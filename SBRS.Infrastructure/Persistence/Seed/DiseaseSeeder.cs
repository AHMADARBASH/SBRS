using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Persistence.Seed;

/// <summary>Inserts only missing codes; existing rows are never changed or deleted.</summary>
public class DiseaseSeeder(AppDbContext db, ILogger<DiseaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedCategoriesAsync(ct);
        await db.SaveChangesAsync(ct);

        await SeedDiseasesAsync(ct);
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        var existing = (await db.DiseaseCategories.Select(c => c.Code).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        foreach (var (code, arName, enName) in DiseaseCategorySeed.Rows)
        {
            if (existing.Contains(code.Trim()))
                continue;

            db.DiseaseCategories.Add(new DiseaseCategoryEntity(arName.Trim(), enName.Trim(), code.Trim()));
            inserted++;
        }

        Log(nameof(DiseaseCategoryEntity), inserted, DiseaseCategorySeed.Rows.Count);
    }

    private async Task SeedDiseasesAsync(CancellationToken ct)
    {
        var categories = (await db.DiseaseCategories.ToListAsync(ct))
            .ToDictionary(c => c.Code.Trim(), StringComparer.OrdinalIgnoreCase);

        var existing = (await db.Diseases.Select(d => d.Code).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        foreach (var (code, arName, enName, categoryCode) in DiseaseSeed.Rows)
        {
            if (existing.Contains(code.Trim()))
                continue;

            if (!categories.TryGetValue(categoryCode.Trim(), out var category))
                throw new InvalidOperationException(
                    $"Disease category '{categoryCode.Trim()}' is missing; cannot seed disease '{code.Trim()}'.");

            db.Diseases.Add(new DiseaseEntity(arName.Trim(), enName.Trim(), code.Trim(), category.Id));
            inserted++;
        }

        Log(nameof(DiseaseEntity), inserted, DiseaseSeed.Rows.Count);
    }

    private void Log(string entity, int inserted, int total) =>
        logger.LogInformation("Reference data {Entity}: {Inserted} inserted of {Total} defined rows.",
            entity, inserted, total);
}

public static class DiseaseCategorySeed
{
    public static readonly IReadOnlyList<(string Code, string ArName, string EnName)> Rows =
    [
        ("A", "عام وغير محدد", "General and unspecified"),
        ("B", "الاعضاء وآلية المناعة", "Organs and immune mechanism"),
        ("C", "العلاج الفيزيائي", "Physiotherapy"),
        ("D", "هضمية", "Digestive"),
        ("F", "العين", "Eye"),
        ("H", "الاذن", "Ear"),
        ("K", "القلب والأوعية الدموية", "Cardiovascular"),
        ("L", "العضلات والعظام", "Musculoskeletal"),
        ("N", "الجهاز العصبي", "Neurological"),
        ("P", "نفسية", "Psychological"),
        ("R", "الجهاز التنفسي", "Respiratory"),
        ("S", "الجلد", "Skin"),
        ("T", "الغدد الصم/ التمثيل الغذائي والتغذوي", "Endocrine/Metabolic and nutritional"),
        ("U", "البولية", "Urologic"),
        ("W", "الحمل, الولادة, تنظيم الأسرة", "Pregnancy, Childbearing, Family Planning"),
        ("X", "الجهاز التناسلي للإناث", "Female Genital"),
        ("Y", "الجهاز التناسلي الذكري", "Male Genital"),
        ("Z", "مشكلات اجتماعية", "Social Problems"),
    ];
}

public static class DiseaseSeed
{
    public static readonly IReadOnlyList<(string Code, string ArName, string EnName, string CategoryCode)> Rows =
    [
        ("A01", "ألم عام/ أماكن متعددة", "Pain general/multiple sites", "A"),
    ];
}
