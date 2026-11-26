using FluentValidation;
using MediatR;
using SBRS.Application.Common.Models;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Queries;

public sealed record GetBeneficiaryConsultationsQuery(
    Guid FacilityId, Guid? VisitId, Guid? ClinicId, Guid? DoctorId, int Page)
    : IRequest<PagedResult<BeneficiaryConsultationDto>>;

public sealed class GetBeneficiaryConsultationsQueryValidator
    : AbstractValidator<GetBeneficiaryConsultationsQuery>
{
    public GetBeneficiaryConsultationsQueryValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100_000);
    }
}

public sealed class GetBeneficiaryConsultationsQueryHandler(
    IBeneficiaryConsultationRepository consultations)
    : IRequestHandler<GetBeneficiaryConsultationsQuery, PagedResult<BeneficiaryConsultationDto>>
{
    private const int PageSize = 30;

    public async Task<PagedResult<BeneficiaryConsultationDto>> Handle(
        GetBeneficiaryConsultationsQuery q, CancellationToken ct)
    {
        var (items, totalCount) = await consultations.GetPagedAsync(
            q.FacilityId, q.VisitId, q.ClinicId, q.DoctorId, q.Page, PageSize, ct);

        return new PagedResult<BeneficiaryConsultationDto>(
            items.Select(c => c.ToDto()).ToList(), q.Page, PageSize, totalCount);
    }
}
