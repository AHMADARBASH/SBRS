using SBRS.Application.Features.Users.DTOs;

namespace SBRS.Application.Common.Interfaces;

public interface IUserAdminService
{
    Task<UserDto> CreateAsync(
        string userName, string password, string firstName, string lastName,
        string role, Guid? organizationId, Guid? facilityId, bool isMobile,
        CancellationToken ct);

    Task<UserDto> UpdateAsync(
        Guid id, string firstName, string lastName,
        string role, Guid? organizationId, Guid? facilityId, bool isMobile,
        CancellationToken ct);

    Task DeactivateAsync(Guid id, CancellationToken ct);

    Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken ct);
}