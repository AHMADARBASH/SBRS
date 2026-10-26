using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;

namespace SBRS.Application.Features.Beneficiaries.Mappings;

public static class BeneficiaryMappings
{
    public static BeneficiaryIdentityDto ToDto(this BeneficiaryIdentityEntity i) =>
        new(i.Id, i.IdentityTypeId, i.IdentityType.Code, i.IdentityType.ArName, i.IdentityType.EnName, i.Value);

    // A beneficiary must always have an identity; a missing one is a data problem, not a normal case.
    public static BeneficiaryIdentityEntity RequireIdentity(
        this IReadOnlyDictionary<Guid, BeneficiaryIdentityEntity> identities, BeneficiaryEntity beneficiary) =>
        identities.TryGetValue(beneficiary.Id, out var identity)
            ? identity
            : throw new DomainException($"Beneficiary {beneficiary.FullCode} has no identity record.");

    public static BeneficiaryMatchDto ToDto(this BeneficiaryMatch m, BeneficiaryIdentityEntity identity) =>
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
            m.Reasons,
            identity.ToDto());

    public static BeneficiarySearchItemDto ToSearchItemDto(this BeneficiaryEntity b, BeneficiaryIdentityEntity identity) =>
        new(b.Id, b.FullCode, b.FirstName, b.FatherName, b.LastName,
            b.BirthDate, b.IsBirthDateEstimated, b.Gender, identity.IdentityType.Code);

    public static PendingBeneficiaryDto ToPendingDto(this BeneficiaryEntity b, BeneficiaryIdentityEntity identity) =>
        new(b.Id, b.FullCode, b.FirstName, b.FatherName, b.LastName, b.MotherName,
            b.BirthDate, b.IsBirthDateEstimated, b.Gender, b.EnrollmentDate, b.CreatedAt, identity.ToDto());

    public static BeneficiaryListItemDto ToListItemDto(this BeneficiaryEntity b, BeneficiaryIdentityEntity identity) =>
        new(b.Id, b.FullCode, b.FirstName, b.FatherName, b.LastName, b.MotherName,
            b.BirthDate, b.IsBirthDateEstimated, b.Gender, b.EnrollmentDate, b.AreaId, b.MatchReviewStatus,
            identity.ToDto());
}
