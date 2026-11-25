namespace SBRS.Application.Common.DTOs;

/// <summary>
/// Compact facility-category reference embedded inside other responses (facilities...).
/// Same shape as the other embedded references; the full <c>FacilityCategoryDto</c>
/// stays for the categories screens.
/// </summary>
public sealed record FacilityCategorySummaryDto(
    Guid Id,
    string Code,
    string ArName,
    string EnName);
