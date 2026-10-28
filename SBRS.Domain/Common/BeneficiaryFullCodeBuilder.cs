using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Common;

/// <summary>
/// Builds the beneficiary FullCode: <c>{FacilityCode}-{Code}</c>, e.g. DAM-PHC-001-57.
/// The facility code (CITY-TYPE-NNN) comes from the registering facility; Code is the
/// plain per-facility number with no padding. FullCode is unique across the whole system.
/// </summary>
public static class BeneficiaryFullCodeBuilder
{
    public static string Build(string facilityCode, int code)
    {
        if (string.IsNullOrWhiteSpace(facilityCode))
            throw new DomainException("The facility code is required.");

        if (code <= 0)
            throw new DomainException("The beneficiary code must be positive.");

        return $"{facilityCode.Trim()}-{code}";
    }
}
