using SBRS.Domain.Exceptions;

namespace SBRS.Domain.Common;

/// <summary>
/// Builds the facility code: <c>{CityCCode}-{FacilityTypeCode}-{Sequence}</c>, e.g. DAM-PHC-001.
/// The sequence is counted per city (not per system, not per type). It is zero-padded
/// to 3 digits but may grow beyond 3 digits and is never truncated.
/// </summary>
public static class FacilityCodeBuilder
{
    public static string Build(string cityCCode, string facilityTypeCode, int sequence)
    {
        if (string.IsNullOrWhiteSpace(cityCCode))
            throw new DomainException("The city c_code is required.");

        if (string.IsNullOrWhiteSpace(facilityTypeCode))
            throw new DomainException("The facility type code is required.");

        if (sequence <= 0)
            throw new DomainException("The facility sequence must be positive.");

        return $"{cityCCode.Trim().ToUpperInvariant()}-{facilityTypeCode.Trim().ToUpperInvariant()}-{sequence.ToString("D3")}";
    }
}
