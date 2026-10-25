using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Features.Beneficiaries.DTOs;
using SBRS.Application.Features.Beneficiaries.Mappings;
using SBRS.Application.Features.Beneficiaries.Services;
using SBRS.Domain.Common;
using SBRS.Domain.Entities;
using SBRS.Domain.Enums;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Beneficiaries.Queries;

public sealed record GetMatchCandidatesQuery(Guid Id) : IRequest<IReadOnlyList<BeneficiaryMatchDto>>;

public sealed class GetMatchCandidatesQueryHandler(
    IBeneficiaryRepository repository, IBeneficiaryMatchService matchService, ICurrentUser currentUser)
    : IRequestHandler<GetMatchCandidatesQuery, IReadOnlyList<BeneficiaryMatchDto>>
{
    public async Task<IReadOnlyList<BeneficiaryMatchDto>> Handle(GetMatchCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var beneficiary = await repository.GetByIdAsync(request.Id, cancellationToken);

        // Out-of-scope records look like missing ones (same rule as the other features).
        if (beneficiary is null ||
            (!currentUser.IsSystemAdmin && beneficiary.RegistrationFacilityId != currentUser.FacilityId))
            throw new NotFoundException(nameof(BeneficiaryEntity), request.Id);

        if (beneficiary.MatchReviewStatus != BeneficiaryMatchReviewStatus.PendingReview)
            throw new DomainException("This beneficiary has no pending match review.");

        var key = BeneficiaryIdentityKey.From(beneficiary.FirstName, beneficiary.FatherName,
            beneficiary.LastName, beneficiary.MotherName, beneficiary.BirthDate);

        var result = await matchService.FindAsync(key, beneficiary.IsBirthDateEstimated, beneficiary.Id, cancellationToken);
        return result.Matches.Select(m => m.ToDto()).ToList();
    }
}
