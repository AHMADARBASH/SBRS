namespace SBRS.Application.Common.DTOs;

/// <summary>
/// Compact facility-type reference embedded inside other responses (me, facilities...).
/// <see cref="Code"/> is the stable routing key for clients — never route on ids or names.
/// </summary>
public sealed record FacilityTypeSummaryDto(
    Guid Id,
    string Code,
    string ArName,
    string EnName);
