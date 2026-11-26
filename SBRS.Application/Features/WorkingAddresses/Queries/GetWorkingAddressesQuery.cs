using FluentValidation;
using MediatR;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.WorkingAddresses.DTOs;
using SBRS.Application.Features.WorkingAddresses.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.WorkingAddresses.Queries;

public sealed record GetWorkingAddressesQuery(Guid FacilityId, int Page)
    : IRequest<PagedResult<WorkingAddressDto>>;

public sealed class GetWorkingAddressesQueryValidator : AbstractValidator<GetWorkingAddressesQuery>
{
    public GetWorkingAddressesQueryValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100_000);
    }
}

public sealed class GetWorkingAddressesQueryHandler(IWorkingAddressRepository workingAddresses)
    : IRequestHandler<GetWorkingAddressesQuery, PagedResult<WorkingAddressDto>>
{
    private const int PageSize = 30;

    public async Task<PagedResult<WorkingAddressDto>> Handle(GetWorkingAddressesQuery q, CancellationToken ct)
    {
        var (items, totalCount) = await workingAddresses.GetPagedAsync(
            q.FacilityId, q.Page, PageSize, ct);

        return new PagedResult<WorkingAddressDto>(
            items.Select(r => r.ToDto()).ToList(), q.Page, PageSize, totalCount);
    }
}
