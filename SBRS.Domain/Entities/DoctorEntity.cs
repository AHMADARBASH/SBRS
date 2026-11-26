using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class DoctorEntity : BaseEntity
{
    private DoctorEntity() { }   // EF

    public DoctorEntity(Guid facilityId, string firstName, string lastName)
    {
        if (facilityId == Guid.Empty) throw new DomainException("Facility is required.");

        FacilityId = facilityId;
        SetNames(firstName, lastName);
    }

    public Guid FacilityId { get; private set; }            // immutable
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    public ICollection<ClinicDoctorEntity> ClinicDoctors { get; private set; } = new List<ClinicDoctorEntity>();

    public void Update(string firstName, string lastName) => SetNames(firstName, lastName);

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("The doctor is already deactivated.");

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("The doctor is already activated.");

        IsActive = true;
    }

    private void SetNames(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("First name and last name are required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
}
