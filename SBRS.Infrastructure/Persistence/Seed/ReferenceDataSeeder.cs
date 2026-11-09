using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Constants;
using SBRS.Domain.Entities;
using SBRS.Infrastructure.Persistence;

namespace SBRS.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent reference data for the beneficiary profile step: upsert by Code —
/// missing rows are inserted, ArName/EnName of existing rows are refreshed from code,
/// nothing is ever deleted. Running the app twice never creates duplicates.
/// </summary>
public class ReferenceDataSeeder(AppDbContext db, ILogger<ReferenceDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        // education_issues ships empty: the lookup exists so the profile can reference it
        await UpsertBeneficiaryStatusesAsync(ct);
        await UpsertMaritalStatusesAsync(ct);
        await UpsertDisabilityReasonsAsync(ct);
        await UpsertChronicDiseasesAsync(ct);

        await db.SaveChangesAsync(ct);
    }

    private async Task UpsertBeneficiaryStatusesAsync(CancellationToken ct)
    {
        var byCode = (await db.BeneficiaryStatuses.ToListAsync(ct)).ToDictionary(s => s.Code);
        var inserted = 0;

        foreach (var (code, ar, en) in BeneficiaryStatusSeed.Rows)
        {
            if (byCode.TryGetValue(code, out var row))
            {
                Refresh(row, ar, en, code);
                continue;
            }

            db.BeneficiaryStatuses.Add(new BeneficiaryStatusEntity(ar, en, code));
            inserted++;
        }

        Log(nameof(BeneficiaryStatusEntity), inserted, BeneficiaryStatusSeed.Rows.Count);
    }

    private async Task UpsertMaritalStatusesAsync(CancellationToken ct)
    {
        var byCode = (await db.MaritalStatuses.ToListAsync(ct)).ToDictionary(s => s.Code);
        var inserted = 0;

        foreach (var (code, ar, en) in MaritalStatusSeed.Rows)
        {
            if (byCode.TryGetValue(code, out var row))
            {
                Refresh(row, ar, en, code);
                continue;
            }

            db.MaritalStatuses.Add(new MaritalStatusEntity(ar, en, code));
            inserted++;
        }

        Log(nameof(MaritalStatusEntity), inserted, MaritalStatusSeed.Rows.Count);
    }

    private async Task UpsertDisabilityReasonsAsync(CancellationToken ct)
    {
        var byCode = (await db.DisabilityReasons.ToListAsync(ct)).ToDictionary(r => r.Code);
        var inserted = 0;

        foreach (var (code, ar, en) in DisabilityReasonSeed.Rows)
        {
            if (byCode.TryGetValue(code, out var row))
            {
                Refresh(row, ar, en, code);
                continue;
            }

            db.DisabilityReasons.Add(new DisabilityReasonEntity(ar, en, code));
            inserted++;
        }

        Log(nameof(DisabilityReasonEntity), inserted, DisabilityReasonSeed.Rows.Count);
    }

    private async Task UpsertChronicDiseasesAsync(CancellationToken ct)
    {
        var byCode = (await db.ChronicDiseases.ToListAsync(ct)).ToDictionary(d => d.Code);
        var inserted = 0;

        foreach (var (code, ar, en) in ChronicDiseaseSeed.Rows)
        {
            if (byCode.TryGetValue(code, out var row))
            {
                Refresh(row, ar, en, code);
                continue;
            }

            db.ChronicDiseases.Add(new ChronicDiseaseEntity(ar, en, code));
            inserted++;
        }

        Log(nameof(ChronicDiseaseEntity), inserted, ChronicDiseaseSeed.Rows.Count);
    }

    /// <summary>Existing rows keep their generated id; only the display names follow the code definition.</summary>
    private static void Refresh<TEntity>(TEntity row, string arName, string enName, string code)
        where TEntity : class
    {
        switch (row)
        {
            case BeneficiaryStatusEntity s when s.ArName != arName || s.EnName != enName:
                s.Update(arName, enName, code);
                break;
            case MaritalStatusEntity s when s.ArName != arName || s.EnName != enName:
                s.Update(arName, enName, code);
                break;
            case DisabilityReasonEntity s when s.ArName != arName || s.EnName != enName:
                s.Update(arName, enName, code);
                break;
            case ChronicDiseaseEntity s when s.ArName != arName || s.EnName != enName:
                s.Update(arName, enName, code);
                break;
        }
    }

    private void Log(string entity, int inserted, int total) =>
        logger.LogInformation("Reference data {Entity}: {Inserted} inserted of {Total} defined rows.",
            entity, inserted, total);
}

/// <summary>Seed values exactly as specified by the product brief; the codes are the stable keys used by the business rules.</summary>
public static class BeneficiaryStatusSeed
{
    public static readonly IReadOnlyList<(string Code, string ArName, string EnName)> Rows =
    [
        (BeneficiaryStatusCodes.NotAvailable, "غير متوفر", "Not available"),
        (BeneficiaryStatusCodes.FormerDetainee, "معتقل سابق", "Former detainee"),
        (BeneficiaryStatusCodes.Missing, "مفقود", "Missing"),
    ];
}

public static class MaritalStatusSeed
{
    public static readonly IReadOnlyList<(string Code, string ArName, string EnName)> Rows =
    [
        (MaritalStatusCodes.Single, "أعزب", "Single"),
        (MaritalStatusCodes.Married, "متزوج", "Married"),
        (MaritalStatusCodes.Divorced, "مطلق", "Divorced"),
        (MaritalStatusCodes.Widowed, "أرمل", "Widowed"),
    ];
}

public static class DisabilityReasonSeed
{
    public static readonly IReadOnlyList<(string Code, string ArName, string EnName)> Rows =
    [
        ("MINE", "لغم", "Mine"),
        ("GENETIC", "وراثي", "Genetic"),
        ("DIABETES", "السكري", "Diabetes"),
        ("VASCULAR", "وعائي", "Vascular"),
        ("CONGENITAL", "خلقي", "Congenital"),
        ("DISEASES", "أمراض", "Diseases"),
        ("ACCIDENTS", "حوادث", "Accidents"),
        ("ERW", "مخلفات الحرب القابلة للانفجار", "Explosive remnants of war (ERW)"),
    ];
}

public static class ChronicDiseaseSeed
{
    public static readonly IReadOnlyList<(string Code, string ArName, string EnName)> Rows =
    [
        ("HEART_DISEASE", "أمراض القلب", "Heart diseases"),
        ("DIABETES", "داء السكري", "Diabetes"),
        ("CANCER", "السرطان", "Cancer"),
        ("CHRONIC_RESPIRATORY", "الأمراض التنفسية المزمنة", "Chronic respiratory diseases"),
        ("ASTHMA", "الربو", "Asthma"),
        ("COPD", "الانسداد الرئوي المزمن", "Chronic obstructive pulmonary disease (COPD)"),
        ("HYPERTENSION", "ارتفاع ضغط الدم", "Hypertension"),
        ("HIGH_CHOLESTEROL", "ارتفاع الكولسترول", "High cholesterol"),
        ("OBESITY", "السمنة", "Obesity"),
        ("OSTEOPOROSIS", "هشاشة العظام", "Osteoporosis"),
        ("RHEUMATOID_ARTHRITIS", "التهاب المفاصل الروماتويدي", "Rheumatoid arthritis"),
        ("CHRONIC_KIDNEY_FAILURE", "الفشل الكلوي المزمن", "Chronic kidney failure"),
        ("CHRONIC_LIVER_DISEASE", "أمراض الكبد المزمنة", "Chronic liver diseases"),
        ("LIVER_CIRRHOSIS", "تليف الكبد", "Liver cirrhosis"),
        ("CHRONIC_ANEMIA", "فقر الدم المزمن", "Chronic anemia"),
        ("EPILEPSY", "الصرع", "Epilepsy"),
        ("ALZHEIMERS", "الزهايمر", "Alzheimer's disease"),
    ];
}
