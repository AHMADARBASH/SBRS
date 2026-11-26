using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Queries;

public sealed record GetBeneficiaryConsultationByIdQuery(Guid Id, Guid FacilityId)
    : IRequest<BeneficiaryConsultationDto>;

public sealed class GetBeneficiaryConsultationByIdQueryValidator
    : AbstractValidator<GetBeneficiaryConsultationByIdQuery>
{
    public GetBeneficiaryConsultationByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

public sealed class GetBeneficiaryConsultationByIdQueryHandler(
    IBeneficiaryConsultationRepository consultations)
    : IRequestHandler<GetBeneficiaryConsultationByIdQuery, BeneficiaryConsultationDto>
{
    public async Task<BeneficiaryConsultationDto> Handle(
        GetBeneficiaryConsultationByIdQuery q, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(q.Id, q.FacilityId, ct)
                           ?? throw new NotFoundException(nameof(BeneficiaryConsultationEntity), q.Id);

        return consultation.ToDto();
    }
}
