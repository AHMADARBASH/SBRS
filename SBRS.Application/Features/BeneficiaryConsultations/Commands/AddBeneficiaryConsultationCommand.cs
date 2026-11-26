using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.BeneficiaryConsultations.DTOs;
using SBRS.Application.Features.BeneficiaryConsultations.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations.Commands;

public sealed record AddBeneficiaryConsultationCommand(
    Guid FacilityId,
    DateTime ConsultationDate,
    Guid VisitId,
    Guid ClinicId,
    Guid DoctorId) : IRequest<BeneficiaryConsultationDto>;

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
    }
}

public sealed class AddBeneficiaryConsultationCommandHandler(
    IVisitRepository visits,
    IClinicRepository clinics,
    IDoctorRepository doctors,
    IClinicDoctorRepository links,
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

        var consultation = new BeneficiaryConsultationEntity(
            c.ConsultationDate, visit.Id, clinic.Id, doctor.Id);
        await consultations.AddAsync(consultation, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return consultation.ToDto(clinic, doctor);
    }
}
