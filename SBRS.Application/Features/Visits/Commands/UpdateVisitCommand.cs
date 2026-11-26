using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Visits.DTOs;
using SBRS.Application.Features.Visits.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Visits.Commands;

public sealed record UpdateVisitCommand(
    Guid Id,
    Guid FacilityId,
    DateOnly? VisitDate,
    decimal? Weight,
    decimal? Height,
    bool? IsPregnant) : IRequest<VisitDto>;

public sealed class UpdateVisitCommandValidator : AbstractValidator<UpdateVisitCommand>
{
    public UpdateVisitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
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

public sealed class UpdateVisitCommandHandler(
    IVisitRepository visits,
    IBeneficiaryRepository beneficiaries,
    IFacilityBeneficiaryRepository enrollments,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateVisitCommand, VisitDto>
{
    public async Task<VisitDto> Handle(UpdateVisitCommand c, CancellationToken ct)
    {
        var visit = await visits.GetByIdAsync(c.Id, c.FacilityId, ct)
                    ?? throw new NotFoundException(nameof(VisitEntity), c.Id);

        var beneficiary = await VisitRules.GetEligibleBeneficiaryAsync(
            visit.BeneficiaryId, c.FacilityId, beneficiaries, enrollments, ct);

        var visitDate = c.VisitDate ?? VisitsDate.Today;
        VisitRules.EnsureVisitDate(visitDate, beneficiary);
        VisitRules.EnsurePregnancy(beneficiary, c.IsPregnant);

        if (await visits.ExistsActiveOnDateAsync(
                c.FacilityId, visit.BeneficiaryId, visitDate, c.Id, ct))
            throw new ConflictException(
                $"A visit already exists for this beneficiary at this facility on {visitDate:yyyy-MM-dd}.");

        visit.Update(visitDate, c.Weight, c.Height, c.IsPregnant);
        await unitOfWork.SaveChangesAsync(ct);

        return visit.ToDto(beneficiary);
    }
}
