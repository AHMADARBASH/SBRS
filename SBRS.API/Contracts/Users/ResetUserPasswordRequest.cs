namespace SBRS.Contracts.Users;

public sealed record ResetUserPasswordRequest(Guid id,string NewPassword);