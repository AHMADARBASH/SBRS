using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Visits.DTOs;
using SBRS.Application.Features.Visits.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Visits.Queries;

public sealed record GetVisitByIdQuery(Guid Id, Guid FacilityId) : IRequest<VisitDto>;

public sealed class GetVisitByIdQueryValidator : AbstractValidator<GetVisitByIdQuery>
{
    public GetVisitByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class GetVisitByIdQueryHandler(IVisitRepository visits)
    : IRequestHandler<GetVisitByIdQuery, VisitDto>
{
    public async Task<VisitDto> Handle(GetVisitByIdQuery q, CancellationToken ct)
    {
        var visit = await visits.GetByIdAsync(q.Id, q.FacilityId, ct)
                    ?? throw new NotFoundException(nameof(VisitEntity), q.Id);

        return visit.ToDto();
    }
}
