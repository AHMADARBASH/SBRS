using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Diseases.DTOs;
using SBRS.Application.Features.Diseases.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Diseases.Queries;

public sealed record GetDiseaseByIdQuery(Guid Id) : IRequest<DiseaseDto>;

public sealed class GetDiseaseByIdQueryHandler(IDiseaseRepository diseases)
    : IRequestHandler<GetDiseaseByIdQuery, DiseaseDto>
{
    public async Task<DiseaseDto> Handle(GetDiseaseByIdQuery q, CancellationToken ct)
    {
        var disease = await diseases.GetByIdAsync(q.Id, ct)
                      ?? throw new NotFoundException(nameof(DiseaseEntity), q.Id);

        return disease.ToDto();
    }
}
