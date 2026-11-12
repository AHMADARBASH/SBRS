namespace SBRS.Application.Common.DTOs;

/// <summary>
/// Compact organization reference embedded inside other responses (me, facilities, branches...).
/// The full <c>OrganizationDto</c> (with audit timestamps) stays for the organizations screens.
/// </summary>
public sealed record OrganizationSummaryDto(
    Guid Id,
    string Code,
    string ArName,
    string EnName,
    bool IsActive);
