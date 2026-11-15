using SBRS.Application.Common.Models;
using SBRS.Application.Features.Users.DTOs;

namespace SBRS.Application.Common.Interfaces;

public interface IUserAdminService
{
    /// <summary>
    /// Paged, filterable user list for the SystemAdmin screen. Inactive users are
    /// hidden unless <paramref name="includeInactive"/> is set.
    /// </summary>
    Task<PagedResult<UserListItemDto>> GetPagedAsync(
        int page, int pageSize, bool includeInactive, string? search, string? role,
        CancellationToken ct);

    Task<UserDto> CreateAsync(
        string userName, string password, string firstName, string lastName,
        string role, Guid? organizationId, Guid? facilityId, bool isMobile,
        CancellationToken ct);

    Task<UserDto> UpdateAsync(
        Guid id, string firstName, string lastName,
        string role, Guid? organizationId, Guid? facilityId, bool isMobile,
        CancellationToken ct);

    Task DeactivateAsync(Guid id, CancellationToken ct);

    Task ActivateAsync(Guid id, CancellationToken ct);

    Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken ct);
}