using FluentValidation;
using MediatR;
using SBRS.Application.Features.Visits.DTOs;
using SBRS.Application.Features.Visits.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Visits.Commands;

public sealed record AddVisitCommand(
    Guid FacilityId,
    Guid BeneficiaryId,
    DateOnly? VisitDate,
    decimal? Weight,
    decimal? Height,
    bool? IsPregnant) : IRequest<VisitDto>;

public sealed class AddVisitCommandValidator : AbstractValidator<AddVisitCommand>
{
    public AddVisitCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.BeneficiaryId).NotEmpty();
        RuleFor(x => x.VisitDate)
            .Must(d => d is null || d <= VisitsDate.Today)
            .WithMessage("Visit date cannot be in the future.");
        RuleFor(x => x.Weight)
            .Must(w => w is null or (>= 0.5m and <= 200m))
            .WithMessage("Weight must be between 0.5 and 200 kg.");
        RuleFor(x => x.Height)
            .Must(h => h is null or (>= 20m and <= 250m))
            .WithMessage("Height must be between 20 and 250 cm.");
    }
}

public sealed class AddVisitCommandHandler(
    IVisitRepository visits,
    IBeneficiaryRepository beneficiaries,
    IFacilityBeneficiaryRepository enrollments,
    IUnitOfWork unitOfWork) : IRequestHandler<AddVisitCommand, VisitDto>
{
    public async Task<VisitDto> Handle(AddVisitCommand c, CancellationToken ct)
    {
        var visitDate = c.VisitDate ?? VisitsDate.Today;
        var beneficiary = await VisitRules.GetEligibleBeneficiaryAsync(
            c.BeneficiaryId, c.FacilityId, beneficiaries, enrollments, ct);

        VisitRules.EnsureVisitDate(visitDate, beneficiary);
        VisitRules.EnsurePregnancy(beneficiary, c.IsPregnant);

        var visit = new VisitEntity(c.FacilityId, beneficiary.Id, visitDate, c.Weight, c.Height, c.IsPregnant);
        await visits.AddAsync(visit, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return visit.ToDto(beneficiary);
    }
}
