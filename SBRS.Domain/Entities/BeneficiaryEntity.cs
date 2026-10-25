using SBRS.Domain.Common;
using SBRS.Domain.Enums;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class BeneficiaryEntity : BaseEntity
{
    public string FirstName { get; private set; } = default!;
    public string FatherName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string MotherName { get; private set; } = default!;
    public DateOnly BirthDate { get; private set; }
    public bool IsBirthDateEstimated { get; private set; }

    /// <summary>true (1) = Male, false (0) = Female.</summary>
    public bool Gender { get; private set; }
    public DateOnly EnrollmentDate { get; private set; }

    // Derived matching columns (never set from outside)
    public string NormalizedFirstName { get; private set; } = default!;
    public string NormalizedFatherName { get; private set; } = default!;
    public string NormalizedLastName { get; private set; } = default!;
    public string NormalizedMotherName { get; private set; } = default!;
    public string MatchKey { get; private set; } = default!;
    public int Disambiguator { get; private set; }

    // Match review workflow
    public BeneficiaryMatchReviewStatus MatchReviewStatus { get; private set; }
    public Guid? MergedIntoBeneficiaryId { get; private set; }
    public string? DistinctReason { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }

    public int Code { get; private set; }
    public string FullCode { get; private set; } = default!;
    public Guid RegistrationFacilityId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private BeneficiaryEntity() { }

    public BeneficiaryEntity(string firstName, string fatherName, string lastName, string motherName,
        DateOnly birthDate, bool isBirthDateEstimated, bool gender, DateOnly enrollmentDate,
        int code, string fullCode, Guid registrationFacilityId,
        int disambiguator, BeneficiaryMatchReviewStatus matchReviewStatus)
    {
        if (code <= 0) throw new DomainException("Code must be positive.");
        if (string.IsNullOrWhiteSpace(fullCode)) throw new DomainException("Full code is required.");
        if (registrationFacilityId == Guid.Empty) throw new DomainException("Registration facility is required.");
        if (disambiguator < 0) throw new DomainException("Disambiguator cannot be negative.");
        if (matchReviewStatus is not (BeneficiaryMatchReviewStatus.NoMatchFound or BeneficiaryMatchReviewStatus.PendingReview))
            throw new DomainException("A new beneficiary must start as NoMatchFound or PendingReview.");
        if (disambiguator > 0 && matchReviewStatus != BeneficiaryMatchReviewStatus.PendingReview)
            throw new DomainException("A beneficiary with the same identity key as another one must start as PendingReview.");
        if (enrollmentDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            throw new DomainException("Enrollment date cannot be in the future.");

        EnrollmentDate = enrollmentDate;
        Code = code;
        FullCode = fullCode.Trim();
        RegistrationFacilityId = registrationFacilityId;
        Disambiguator = disambiguator;
        MatchReviewStatus = matchReviewStatus;

        UpdateIdentity(firstName, fatherName, lastName, motherName, birthDate, isBirthDateEstimated, gender);
    }

    public void UpdateIdentity(string firstName, string fatherName, string lastName, string motherName,
        DateOnly birthDate, bool isBirthDateEstimated, bool gender)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(fatherName)) throw new DomainException("Father name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(motherName)) throw new DomainException("Mother name is required.");

        var resolvedBirthDate = BeneficiaryBirthDate.Resolve(birthDate, isBirthDateEstimated);
        if (resolvedBirthDate > DateOnly.FromDateTime(DateTime.UtcNow)) throw new DomainException("Birth date cannot be in the future.");

        FirstName = firstName.Trim();
        FatherName = fatherName.Trim();
        LastName = lastName.Trim();
        MotherName = motherName.Trim();
        BirthDate = resolvedBirthDate;
        IsBirthDateEstimated = isBirthDateEstimated;
        Gender = gender;

        var key = BeneficiaryIdentityKey.From(FirstName, FatherName, LastName, MotherName, BirthDate);
        NormalizedFirstName = key.FirstName;
        NormalizedFatherName = key.FatherName;
        NormalizedLastName = key.LastName;
        NormalizedMotherName = key.MotherName;
        MatchKey = key.MatchKey;
    }

    /// <summary>The facility admin checked the candidates and this is a different person.</summary>
    public void ConfirmNotAMatch(string? reason, Guid reviewedBy)
    {
        EnsurePendingReview();

        MatchReviewStatus = BeneficiaryMatchReviewStatus.ConfirmedDistinct;
        DistinctReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
    }

    /// <summary>The facility admin selected the existing beneficiary this record duplicates.</summary>
    public void MergeInto(Guid existingBeneficiaryId, Guid reviewedBy)
    {
        EnsurePendingReview();
        if (existingBeneficiaryId == Guid.Empty || existingBeneficiaryId == Id)
            throw new DomainException("Invalid beneficiary selected.");

        MatchReviewStatus = BeneficiaryMatchReviewStatus.MergedIntoExisting;
        MergedIntoBeneficiaryId = existingBeneficiaryId;
        IsActive = false;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
    }

    public void Deactivate() => IsActive = false;

    private void EnsurePendingReview()
    {
        if (MatchReviewStatus != BeneficiaryMatchReviewStatus.PendingReview)
            throw new DomainException("This beneficiary has no pending match review.");
    }
}
