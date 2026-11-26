using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Mappings;
using SBRS.Application.Features.Diseases.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Commands;

public sealed record AddBeneficiaryConsultationCommand(
    Guid FacilityId,
    DateTime ConsultationDate,
    Guid VisitId,
    Guid ClinicId,
    Guid DoctorId,
    IReadOnlyList<Guid>? DiseaseIds) : IRequest<BeneficiaryConsultationDto>;

public sealed class AddBeneficiaryConsultationCommandValidator
    : AbstractValidator<AddBeneficiaryConsultationCommand>
{
    public AddBeneficiaryConsultationCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
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

public sealed class AddBeneficiaryConsultationCommandHandler(
    IVisitRepository visits,
    IClinicRepository clinics,
    IDoctorRepository doctors,
    IClinicDoctorRepository links,
    IDiseaseRepository diseaseRepository,
    IBeneficiaryConsultationRepository consultations,
    IUnitOfWork unitOfWork) : IRequestHandler<AddBeneficiaryConsultationCommand, BeneficiaryConsultationDto>
{
    public async Task<BeneficiaryConsultationDto> Handle(AddBeneficiaryConsultationCommand c, CancellationToken ct)
    {
        var visit = await visits.GetByIdAsync(c.VisitId, c.FacilityId, ct)
                    ?? throw new NotFoundException(nameof(VisitEntity), c.VisitId);

        var (clinic, doctor) = await ConsultationRules.GetEligibleClinicAndDoctorAsync(
            c.ClinicId, c.DoctorId, c.FacilityId, clinics, doctors, links, ct);

        if (await consultations.ExistsAsync(visit.Id, clinic.Id, doctor.Id, null, ct))
            throw new ConflictException("This consultation already exists for the visit, clinic and doctor.");

        var diseases = await ConsultationRules.GetDiseasesAsync(c.DiseaseIds ?? [], diseaseRepository, ct);

        var consultation = new BeneficiaryConsultationEntity(
            c.ConsultationDate, visit.Id, clinic.Id, doctor.Id);
        await consultations.AddAsync(consultation, ct);

        foreach (var disease in diseases)
            await consultations.AddDetailAsync(
                new BeneficiaryConsultationDetailEntity(consultation.Id, disease.Id), ct);

        await unitOfWork.SaveChangesAsync(ct);

        return consultation.ToDto(clinic, doctor, diseases.Select(d => d.ToDto()).ToList());
    }
}
