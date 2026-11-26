using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Commands;

public sealed record UpdateBeneficiaryConsultationCommand(
    Guid Id,
    Guid FacilityId,
    DateTime ConsultationDate,
    Guid ClinicId,
    Guid DoctorId) : IRequest<BeneficiaryConsultationDto>;

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
    }
}

public sealed class UpdateBeneficiaryConsultationCommandHandler(
    IVisitRepository visits,
    IClinicRepository clinics,
    IDoctorRepository doctors,
    IClinicDoctorRepository links,
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

        consultation.Update(c.ConsultationDate, clinic.Id, doctor.Id);
        await unitOfWork.SaveChangesAsync(ct);

        return consultation.ToDto(clinic, doctor);
    }
}
