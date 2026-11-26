using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Entities;

public class BeneficiaryIdentityEntity : BaseEntity
{
    public Guid BeneficiaryId { get; private set; }
    public Guid IdentityTypeId { get; private set; }
    public string Value { get; private set; } = string.Empty;

    // Loaded with Include() when a query needs the type; never assigned in the constructor.
    public IdentityTypeEntity IdentityType { get; private set; } = default!;

    private BeneficiaryIdentityEntity() { }

    public BeneficiaryIdentityEntity(Guid beneficiaryId, IdentityTypeEntity identityType, string? value)
    {
        if (beneficiaryId == Guid.Empty) throw new DomainException("Beneficiary is required.");
        ArgumentNullException.ThrowIfNull(identityType);

        var normalized = IdentityValueNormalizer.Normalize(value);
        if (!identityType.IsValidValue(normalized))
            throw new DomainException($"The identity value is not valid for the identity type {identityType.Code}.");

        BeneficiaryId = beneficiaryId;
        IdentityTypeId = identityType.Id;
        Value = normalized;
    }

    // Updates the primary identity row in place; the type's regex is applied to the normalized value.
    public void Update(IdentityTypeEntity identityType, string? value)
    {
        ArgumentNullException.ThrowIfNull(identityType);

        var normalized = IdentityValueNormalizer.Normalize(value);
        if (!identityType.IsValidValue(normalized))
            throw new DomainException($"The identity value is not valid for the identity type {identityType.Code}.");

        IdentityTypeId = identityType.Id;
        Value = normalized;
    }
}
