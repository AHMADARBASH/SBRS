using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Organizations.DTOs;
using SBRS.Application.Features.Organizations.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Organizations.Queries.GetOrganizationById;

public record GetOrganizationByIdQuery(Guid Id) : IRequest<OrganizationDto>;

public class GetOrganizationByIdQueryHandler(IOrganizationRepository organizations)
    : IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(GetOrganizationByIdQuery request, CancellationToken ct)
    {
        var entity = await organizations.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(OrganizationEntity), request.Id);

        return entity.ToDto();
    }
}