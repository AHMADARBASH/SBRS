namespace SBRS.Application.Common.DTOs;

/// <summary>
/// Compact branch reference embedded inside other responses (me, users, facilities...).
/// Same shape as the organization and facility summaries so the client can render any
/// scope object the same way. The full <c>BranchDto</c> (with organization id and audit
/// timestamps) stays for the branches screens.
/// </summary>
public sealed record BranchSummaryDto(
    Guid Id,
    string Code,
    string ArName,
    string EnName,
    bool IsActive);
