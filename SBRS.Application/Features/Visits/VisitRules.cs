using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Visits;

/// <summary>Rules shared by the add and update handlers.</summary>
public static class VisitRules
{
    public static async Task<BeneficiaryEntity> GetEligibleBeneficiaryAsync(Guid beneficiaryId,
        Guid facilityId, IBeneficiaryRepository beneficiaries,
        IFacilityBeneficiaryRepository enrollments, CancellationToken ct)
    {
        var beneficiary = await beneficiaries.GetByIdAsync(beneficiaryId, ct);

        if (beneficiary is null || !beneficiary.IsActive || beneficiary.MergedIntoBeneficiaryId is not null ||
            !await enrollments.IsEnrolledAsync(beneficiaryId, facilityId, ct))
            throw new NotFoundException(nameof(BeneficiaryEntity), beneficiaryId);

        return beneficiary;
    }

    public static void EnsureVisitDate(DateOnly visitDate, BeneficiaryEntity beneficiary)
    {
        if (visitDate > VisitsDate.Today)
            throw new DomainException("Visit date cannot be in the future.");
        if (visitDate < beneficiary.EnrollmentDate)
            throw new DomainException("Visit date cannot be earlier than the beneficiary's enrollment date.");
    }

    public static void EnsurePregnancy(BeneficiaryEntity beneficiary, bool? isPregnant)
    {
        if (beneficiary.Gender && isPregnant.HasValue)
            throw new DomainException("Pregnancy can only be recorded for a female beneficiary.");
    }
}
