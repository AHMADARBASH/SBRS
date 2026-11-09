using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.BeneficiaryProfiles.DTOs;
using SBRS.Application.Features.BeneficiaryProfiles.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryProfiles.Queries;

/// <summary>
/// Reads the sensitive profile of one beneficiary. The beneficiary must be in scope;
/// when no profile was saved yet this is answered as 404 (frontend offers the form then).
/// </summary>
public sealed record GetBeneficiaryProfileQuery(Guid BeneficiaryId) : IRequest<BeneficiaryProfileDto>;

public sealed class GetBeneficiaryProfileQueryHandler(
    IBeneficiaryRepository beneficiaries,
    IBeneficiaryProfileRepository profiles,
    ICurrentUser currentUser)
    : IRequestHandler<GetBeneficiaryProfileQuery, BeneficiaryProfileDto>
{
    public async Task<BeneficiaryProfileDto> Handle(GetBeneficiaryProfileQuery request,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.GetByIdAsync(request.BeneficiaryId, cancellationToken);
        if (beneficiary is null ||
            (!currentUser.IsSystemAdmin && beneficiary.RegistrationFacilityId != currentUser.FacilityId))
            throw new NotFoundException(nameof(BeneficiaryEntity), request.BeneficiaryId);

        var profile = await profiles.GetByBeneficiaryIdAsync(beneficiary.Id, cancellationToken);
        if (profile is null)
            throw new NotFoundException(nameof(BeneficiaryProfileEntity), beneficiary.Id);

        var disabilities = (await profiles.GetDisabilitiesAsync(beneficiary.Id, cancellationToken))
            .Select(d => d.ToDto()).ToList();
        var chronicIds = (await profiles.GetChronicDiseasesAsync(beneficiary.Id, cancellationToken))
            .Select(c => c.ChronicDiseaseId).ToList();

        return profile.ToDto(disabilities, chronicIds);
    }
}
