namespace SBRS.Application.Features.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName,
    string Role,
    Guid? OrganizationId,
    Guid? BranchId,
    Guid? FacilityId,
    bool IsMobile,
    bool IsActive,
    DateTime CreatedAt);