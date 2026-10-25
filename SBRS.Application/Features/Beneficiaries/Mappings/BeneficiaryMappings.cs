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

    public static BeneficiarySearchItemDto ToSearchItemDto(this BeneficiaryEntity b) =>
        new(b.Id, b.FullCode, b.FirstName, b.FatherName, b.LastName,
            b.BirthDate, b.IsBirthDateEstimated, b.Gender);

    public static PendingBeneficiaryDto ToPendingDto(this BeneficiaryEntity b) =>
        new(b.Id, b.FullCode, b.FirstName, b.FatherName, b.LastName, b.MotherName,
            b.BirthDate, b.IsBirthDateEstimated, b.Gender, b.EnrollmentDate, b.CreatedAt);
}
