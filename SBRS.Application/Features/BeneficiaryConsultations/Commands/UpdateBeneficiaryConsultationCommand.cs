using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Mappings;
using SBRS.Application.Features.Diseases.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Commands;

public sealed record UpdateBeneficiaryConsultationCommand(
    Guid Id,
    Guid FacilityId,
    DateTime ConsultationDate,
    Guid ClinicId,
    Guid DoctorId,
    IReadOnlyList<Guid>? DiseaseIds) : IRequest<BeneficiaryConsultationDto>;

public sealed class UpdateBeneficiaryConsultationCommandValidator
    : AbstractValidator<UpdateBeneficiaryConsultationCommand>
{
    public UpdateBeneficiaryConsultationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ConsultationDate).NotEmpty();
        RuleFor(x => x.DiseaseIds)
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("The same disease cannot be listed twice.");
        RuleForEach(x => x.DiseaseIds)
            .Must(id => id != Guid.Empty)
            .WithMessage("Disease id cannot be empty.");
    }
}

public sealed class UpdateBeneficiaryConsultationCommandHandler(
    IVisitRepository visits,
    IClinicRepository clinics,
    IDoctorRepository doctors,
    IClinicDoctorRepository links,
    IDiseaseRepository diseaseRepository,
    IBeneficiaryConsultationRepository consultations,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBeneficiaryConsultationCommand, BeneficiaryConsultationDto>
{
    public async Task<BeneficiaryConsultationDto> Handle(UpdateBeneficiaryConsultationCommand c, CancellationToken ct)
    {
        var consultation = await consultations.GetByIdAsync(c.Id, c.FacilityId, ct)
                           ?? throw new NotFoundException(nameof(BeneficiaryConsultationEntity), c.Id);

        _ = await visits.GetByIdAsync(consultation.VisitId, c.FacilityId, ct)
            ?? throw new NotFoundException(nameof(VisitEntity), consultation.VisitId);

        var (clinic, doctor) = await ConsultationRules.GetEligibleClinicAndDoctorAsync(
            c.ClinicId, c.DoctorId, c.FacilityId, clinics, doctors, links, ct);

        if (await consultations.ExistsAsync(consultation.VisitId, clinic.Id, doctor.Id, c.Id, ct))
            throw new ConflictException("This consultation already exists for the visit, clinic and doctor.");

        IReadOnlyList<DiseaseEntity> diseases = [];
        if (c.DiseaseIds is not null)
        {
            diseases = await ConsultationRules.GetDiseasesAsync(c.DiseaseIds, diseaseRepository, ct);

            var current = await consultations.GetDetailsByConsultationIdAsync(consultation.Id, ct);
            var byDisease = current.ToDictionary(l => l.DiseaseId);
            var requested = c.DiseaseIds.Distinct().ToHashSet();

            foreach (var link in current.Where(l => l.IsActive && !requested.Contains(l.DiseaseId)))
                link.Deactivate();

            foreach (var id in requested)
            {
                if (byDisease.TryGetValue(id, out var link))
                {
                    if (!link.IsActive)
                        link.Activate();
                }
                else
                {
                    await consultations.AddDetailAsync(
                        new BeneficiaryConsultationDetailEntity(consultation.Id, id), ct);
                }
            }
        }

        consultation.Update(c.ConsultationDate, clinic.Id, doctor.Id);
        await unitOfWork.SaveChangesAsync(ct);

        return c.DiseaseIds is null
            ? consultation.ToDto(clinic, doctor)
            : consultation.ToDto(clinic, doctor, diseases.Select(d => d.ToDto()).ToList());
    }
}
