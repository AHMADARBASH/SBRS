using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Facilities.DTOs;
using SBRS.Application.Features.Facilities.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Facilities.Queries;

public record GetFacilityByIdQuery(Guid Id) : IRequest<FacilityDto>;

public class GetFacilityByIdQueryHandler(IFacilityRepository facilities)
    : IRequestHandler<GetFacilityByIdQuery, FacilityDto>
{
    public async Task<FacilityDto> Handle(GetFacilityByIdQuery request, CancellationToken ct)
    {
        var entity = await facilities.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(FacilityEntity), request.Id);

        return entity.ToDto();
    }
}