using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Queries;

public sealed record GetBeneficiaryConsultationsByVisitIdQuery(Guid VisitId, Guid FacilityId)
    : IRequest<IReadOnlyList<BeneficiaryConsultationDto>>;

public sealed class GetBeneficiaryConsultationsByVisitIdQueryValidator
    : AbstractValidator<GetBeneficiaryConsultationsByVisitIdQuery>
{
    public GetBeneficiaryConsultationsByVisitIdQueryValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class GetBeneficiaryConsultationsByVisitIdQueryHandler(
    IVisitRepository visits, IBeneficiaryConsultationRepository consultations)
    : IRequestHandler<GetBeneficiaryConsultationsByVisitIdQuery, IReadOnlyList<BeneficiaryConsultationDto>>
{
    public async Task<IReadOnlyList<BeneficiaryConsultationDto>> Handle(
        GetBeneficiaryConsultationsByVisitIdQuery q, CancellationToken ct)
    {
        _ = await visits.GetByIdAsync(q.VisitId, q.FacilityId, ct)
            ?? throw new NotFoundException(nameof(VisitEntity), q.VisitId);

        var items = await consultations.GetByVisitIdAsync(q.VisitId, q.FacilityId, ct);
        return items.Select(c => c.ToDto()).ToList();
    }
}
