namespace SBRS.Contracts.Users;

public sealed record CreateUserRequest(
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    string Role,
    Guid? OrganizationId,
    Guid? BranchId,
    Guid? FacilityId,
    bool IsMobile);