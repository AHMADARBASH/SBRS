using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

/// <summary>Reads the registration data of one beneficiary; out-of-scope looks like missing (404).</summary>
public sealed record GetBeneficiaryRegistrationQuery(Guid BeneficiaryId)
    : IRequest<BeneficiaryRegistrationDto>;

public sealed class GetBeneficiaryRegistrationQueryHandler(
    IBeneficiaryRepository beneficiaries,
    IBeneficiaryIdentityRepository identities,
    IFacilityBeneficiaryRepository enrollments,
    ICurrentUser currentUser) : IRequestHandler<GetBeneficiaryRegistrationQuery, BeneficiaryRegistrationDto>
{
    public async Task<BeneficiaryRegistrationDto> Handle(GetBeneficiaryRegistrationQuery request,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.GetByIdAsync(request.BeneficiaryId, cancellationToken);

        var isInScope = currentUser.IsSystemAdmin ||
                        (currentUser.FacilityId is { } facilityId &&
                         await enrollments.IsEnrolledAsync(
                             request.BeneficiaryId, facilityId, cancellationToken));

        if (beneficiary is null || !isInScope)
            throw new NotFoundException(nameof(BeneficiaryEntity), request.BeneficiaryId);

        // GetByBeneficiaryIdAsync returns all rows oldest first; the primary identity is the first.
        var records = await identities.GetByBeneficiaryIdAsync(beneficiary.Id, cancellationToken);
        return beneficiary.ToRegistrationDto(records.FirstOrDefault());
    }
}
