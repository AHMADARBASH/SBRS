using System.Text.RegularExpressions;
using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

/// <summary>
/// Sensitive second registration step of a beneficiary (1:1, created once and then fully replaced):
/// phones, displacement, education, work, marital situation, status, disabilities and chronic
/// diseases (the latter two live in their own child tables).
/// </summary>
public class BeneficiaryProfileEntity : BaseEntity
{
    private static readonly Regex PhonePattern = new(@"^\+?[0-9]{7,15}$", RegexOptions.Compiled);

    private BeneficiaryProfileEntity() { }   // EF

    public BeneficiaryProfileEntity(
        Guid beneficiaryId,
        string? phoneNumber1, string? phoneNumber2,
        bool isDisplaced, DateOnly? displacementDate, Guid? oldAreaId,
        Guid? accommodationTypeId, bool isDisabled, Guid beneficiaryStatusId,
        string? currentWork, string? previousWork,
        Guid? educationTypeId, Guid? educationIssueId,
        Guid? maritalStatusId, short? wivesNumber, short? childrenNumber)
    {
        if (beneficiaryId == Guid.Empty) throw new DomainException("Beneficiary is required.");
        BeneficiaryId = beneficiaryId;
        Set(phoneNumber1, phoneNumber2, isDisplaced, displacementDate, oldAreaId,
            accommodationTypeId, isDisabled, beneficiaryStatusId, currentWork, previousWork,
            educationTypeId, educationIssueId, maritalStatusId, wivesNumber, childrenNumber);
    }

    public Guid BeneficiaryId { get; private set; }                 // immutable (1:1, unique)
    public string? PhoneNumber1 { get; private set; }
    public string? PhoneNumber2 { get; private set; }
    public bool IsDisplaced { get; private set; }
    public DateOnly? DisplacementDate { get; private set; }
    public Guid? OldAreaId { get; private set; }
    public Guid? AccommodationTypeId { get; private set; }          // nullable: the answer may not be known yet
    public bool IsDisabled { get; private set; }
    public Guid BeneficiaryStatusId { get; private set; }           // filled with NOT_AVAILABLE by the handler when omitted
    public string? CurrentWork { get; private set; }
    public string? PreviousWork { get; private set; }
    public Guid? EducationTypeId { get; private set; }              // nullable: the answer may not be known yet
    public Guid? EducationIssueId { get; private set; }
    public Guid? MaritalStatusId { get; private set; }              // nullable: the answer may not be known yet
    public short? WivesNumber { get; private set; }
    public short? ChildrenNumber { get; private set; }

    /// <summary>Full-replace semantics for a saved profile (audit columns of the profile itself are preserved).</summary>
    public void Update(
        string? phoneNumber1, string? phoneNumber2,
        bool isDisplaced, DateOnly? displacementDate, Guid? oldAreaId,
        Guid? accommodationTypeId, bool isDisabled, Guid beneficiaryStatusId,
        string? currentWork, string? previousWork,
        Guid? educationTypeId, Guid? educationIssueId,
        Guid? maritalStatusId, short? wivesNumber, short? childrenNumber) =>
        Set(phoneNumber1, phoneNumber2, isDisplaced, displacementDate, oldAreaId,
            accommodationTypeId, isDisabled, beneficiaryStatusId, currentWork, previousWork,
            educationTypeId, educationIssueId, maritalStatusId, wivesNumber, childrenNumber);

    private void Set(
        string? phoneNumber1, string? phoneNumber2,
        bool isDisplaced, DateOnly? displacementDate, Guid? oldAreaId,
        Guid? accommodationTypeId, bool isDisabled, Guid beneficiaryStatusId,
        string? currentWork, string? previousWork,
        Guid? educationTypeId, Guid? educationIssueId,
        Guid? maritalStatusId, short? wivesNumber, short? childrenNumber)
    {
        // Phones are optional; empty becomes null. The digit conversion happens before the profile gets them.
        var phone1 = NormalizePhone(phoneNumber1);
        var phone2 = NormalizePhone(phoneNumber2);
        ValidatePhones(phone1, phone2);

        // Required lookups. Only the status must always resolve (the handler defaults it to
        // NOT_AVAILABLE); the remaining lookups may be omitted while the answer is unknown.
        if (beneficiaryStatusId == Guid.Empty) throw new DomainException("Beneficiary status is required.");

        // Displacement is consistent on its own: without displacement nothing may remain.
        if (!isDisplaced)
        {
            if (displacementDate is not null || oldAreaId is not null)
                throw new DomainException("A beneficiary that is not displaced cannot have a displacement date or old area.");
        }
        else if (oldAreaId is null || oldAreaId == Guid.Empty)
        {
            throw new DomainException("Old area is required for a displaced beneficiary.");
        }

        if (wivesNumber is < 1 or > 4) throw new DomainException("Wives number must be between 1 and 4.");
        if (childrenNumber is < 0 or > 30) throw new DomainException("Children number must be between 0 and 30.");

        PhoneNumber1 = phone1;
        PhoneNumber2 = phone2;
        IsDisplaced = isDisplaced;
        DisplacementDate = isDisplaced ? displacementDate : null;
        OldAreaId = isDisplaced ? oldAreaId : null;
        AccommodationTypeId = accommodationTypeId;
        IsDisabled = isDisabled;
        BeneficiaryStatusId = beneficiaryStatusId;
        CurrentWork = NormalizeWork(currentWork, nameof(CurrentWork));
        PreviousWork = NormalizeWork(previousWork, nameof(PreviousWork));
        EducationTypeId = educationTypeId;
        EducationIssueId = educationIssueId == Guid.Empty ? null : educationIssueId;
        MaritalStatusId = maritalStatusId;
        WivesNumber = wivesNumber;
        ChildrenNumber = childrenNumber;
    }

    private static string? NormalizePhone(string? phone)
    {
        var trimmed = phone?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static void ValidatePhones(string? phone1, string? phone2)
    {
        if (phone1 is not null && !PhonePattern.IsMatch(phone1))
            throw new DomainException("Phone number 1 is not valid.");
        if (phone2 is not null)
        {
            if (phone1 is null)
                throw new DomainException("Phone number 2 cannot be set without phone number 1.");
            if (phone2 == phone1)
                throw new DomainException("Phone number 2 must differ from phone number 1.");
            if (!PhonePattern.IsMatch(phone2))
                throw new DomainException("Phone number 2 is not valid.");
        }
    }

    private static string? NormalizeWork(string? work, string field)
    {
        var trimmed = work?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        if (trimmed.Length > 200) throw new DomainException($"{field} cannot exceed 200 characters.");
        return trimmed;
    }
}
