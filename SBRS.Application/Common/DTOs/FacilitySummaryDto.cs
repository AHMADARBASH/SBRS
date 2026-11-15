namespace SBRS.Application.Common.DTOs;

/// <summary>
/// Compact facility reference embedded inside other responses (me, users, ...).
/// The full <c>FacilityDto</c> stays for the facilities screens; type/branch details
/// belong to the endpoints that need them (see <c>CurrentFacilityDto</c>).
/// </summary>
public sealed record FacilitySummaryDto(
    Guid Id,
    string Code,
    string ArName,
    string EnName,
    bool IsActive);
