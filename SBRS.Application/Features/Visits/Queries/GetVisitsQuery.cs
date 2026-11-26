using FluentValidation;
using MediatR;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.Visits.DTOs;
using SBRS.Application.Features.Visits.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Visits.Queries;

public sealed record GetVisitsQuery(Guid FacilityId, Guid? BeneficiaryId, int Page)
    : IRequest<PagedResult<VisitDto>>;

public sealed class GetVisitsQueryValidator : AbstractValidator<GetVisitsQuery>
{
    public GetVisitsQueryValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100_000);
    }
}

public sealed class GetVisitsQueryHandler(IVisitRepository visits)
    : IRequestHandler<GetVisitsQuery, PagedResult<VisitDto>>
{
    private const int PageSize = 30;

    public async Task<PagedResult<VisitDto>> Handle(GetVisitsQuery q, CancellationToken ct)
    {
        var (items, totalCount) = await visits.GetPagedAsync(
            q.FacilityId, q.BeneficiaryId, q.Page, PageSize, ct);

        return new PagedResult<VisitDto>(
            items.Select(v => v.ToDto()).ToList(), q.Page, PageSize, totalCount);
    }
}
