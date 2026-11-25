namespace SBRS.Application.Common.DTOs;

/// <summary>
/// Compact area reference embedded inside other responses (facilities...).
/// Same shape as the other embedded references so the client can render any
/// related object the same way. The full <c>AreaDto</c> (with district id and
/// geo hierarchy) stays for the geography endpoints.
/// </summary>
public sealed record AreaSummaryDto(
    Guid Id,
    string Code,
    string ArName,
    string EnName);
