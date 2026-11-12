namespace SBRS.Application.Common.DTOs;

/// <summary>
/// Compact branch reference embedded inside other responses (me, facilities...).
/// The full <c>BranchDto</c> (with organization id and audit timestamps) stays for the branches screens.
/// </summary>
public sealed record BranchSummaryDto(
    Guid Id,
    string ArName,
    string EnName);
