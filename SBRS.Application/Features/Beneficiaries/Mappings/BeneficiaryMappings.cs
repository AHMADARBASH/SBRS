using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.Beneficiaries.Mappings;

public static class BeneficiaryMappings
{
    public static BeneficiaryMatchDto ToDto(this BeneficiaryMatch m) =>
        new(m.Beneficiary.Id,
            m.Beneficiary.FullCode,
            m.Beneficiary.FirstName,
            m.Beneficiary.FatherName,
            m.Beneficiary.LastName,
            m.Beneficiary.MotherName,
            m.Beneficiary.BirthDate,
            m.Beneficiary.IsBirthDateEstimated,
            m.Beneficiary.Gender,
            m.Status,
            m.Score,
            m.Reasons);

    public static BeneficiarySearchResultDto ToDto(this BeneficiaryMatchSearchResult result, bool includeMatches) =>
        new(result.Status,
            result.Matches.Count,
            includeMatches ? result.Matches.Select(m => m.ToDto()).ToList() : Array.Empty<BeneficiaryMatchDto>());

    public static PendingBeneficiaryDto ToPendingDto(this BeneficiaryEntity b) =>
        new(b.Id, b.FullCode, b.FirstName, b.FatherName, b.LastName, b.MotherName,
            b.BirthDate, b.IsBirthDateEstimated, b.Gender, b.EnrollmentDate, b.CreatedAt);
}
