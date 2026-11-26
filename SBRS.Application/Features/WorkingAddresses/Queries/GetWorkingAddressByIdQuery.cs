using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.WorkingAddresses.DTOs;
using SBRS.Application.Features.WorkingAddresses.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.WorkingAddresses.Queries;

public sealed record GetWorkingAddressByIdQuery(Guid Id, Guid FacilityId) : IRequest<WorkingAddressDto>;

public sealed class GetWorkingAddressByIdQueryValidator : AbstractValidator<GetWorkingAddressByIdQuery>
{
    public GetWorkingAddressByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class GetWorkingAddressByIdQueryHandler(IWorkingAddressRepository workingAddresses)
    : IRequestHandler<GetWorkingAddressByIdQuery, WorkingAddressDto>
{
    public async Task<WorkingAddressDto> Handle(GetWorkingAddressByIdQuery q, CancellationToken ct) =>
        (await workingAddresses.GetByIdAsync(q.Id, q.FacilityId, ct)
         ?? throw new NotFoundException(nameof(WorkingAddressEntity), q.Id)).ToDto();
}
