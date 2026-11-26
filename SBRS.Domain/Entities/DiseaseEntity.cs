using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class DiseaseEntity : BaseEntity
{
    private DiseaseEntity() { }   // EF

    public DiseaseEntity(string arName, string enName, string code, Guid diseaseCategoryId)
    {
        Set(arName, enName, code, diseaseCategoryId);
    }

    public string ArName { get; private set; } = default!;
    public string EnName { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public Guid DiseaseCategoryId { get; private set; }

    public DiseaseCategoryEntity DiseaseCategory { get; private set; } = default!;

    public void Update(string arName, string enName, string code, Guid diseaseCategoryId) =>
        Set(arName, enName, code, diseaseCategoryId);

    private void Set(string arName, string enName, string code, Guid diseaseCategoryId)
    {
        if (string.IsNullOrWhiteSpace(arName) || string.IsNullOrWhiteSpace(enName) || string.IsNullOrWhiteSpace(code))
            throw new DomainException("Arabic name, English name and code are required.");
        if (diseaseCategoryId == Guid.Empty)
            throw new DomainException("Disease category is required.");

        ArName = arName.Trim();
        EnName = enName.Trim();
        Code = code.Trim();
        DiseaseCategoryId = diseaseCategoryId;
    }
}
