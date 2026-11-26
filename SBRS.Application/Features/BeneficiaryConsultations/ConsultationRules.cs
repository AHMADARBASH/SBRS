using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.BeneficiaryConsultations;

/// <summary>Rules shared by the add and update handlers.</summary>
public static class ConsultationRules
{
    public static async Task<(ClinicEntity Clinic, DoctorEntity Doctor)> GetEligibleClinicAndDoctorAsync(
        Guid clinicId, Guid doctorId, Guid facilityId,
        IClinicRepository clinics, IDoctorRepository doctors,
        IClinicDoctorRepository links, CancellationToken ct)
    {
        var clinic = await clinics.GetByIdAsync(clinicId, ct)
                     ?? throw new NotFoundException(nameof(ClinicEntity), clinicId);
        if (clinic.FacilityId != facilityId || !clinic.IsActive)
            throw new NotFoundException(nameof(ClinicEntity), clinicId);

        var doctor = await doctors.GetByIdAsync(doctorId, ct)
                     ?? throw new NotFoundException(nameof(DoctorEntity), doctorId);
        if (doctor.FacilityId != facilityId || !doctor.IsActive)
            throw new NotFoundException(nameof(DoctorEntity), doctorId);

        var link = await links.GetAsync(clinicId, doctorId, ct);
        if (link is null || !link.IsActive)
            throw new DomainException("The doctor is not assigned to the selected clinic.");

        return (clinic, doctor);
    }

    public static async Task<IReadOnlyList<DiseaseEntity>> GetDiseasesAsync(IReadOnlyList<Guid> ids,
        IDiseaseRepository diseases, CancellationToken ct)
    {
        var distinct = ids.Distinct().ToList();
        var found = await diseases.GetByIdsAsync(distinct, ct);
        var byId = found.ToDictionary(d => d.Id);

        foreach (var id in distinct)
        {
            if (!byId.ContainsKey(id))
                throw new NotFoundException(nameof(DiseaseEntity), id);
        }

        return found.OrderBy(d => d.Code).ToList();
    }
}
