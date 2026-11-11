namespace SBRS.Contracts.Users;

public sealed record UpdateUserRequest(
    Guid id,
    string FirstName,
    string LastName,
    string Role,
    Guid? OrganizationId,
    Guid? FacilityId,
    bool IsMobile);