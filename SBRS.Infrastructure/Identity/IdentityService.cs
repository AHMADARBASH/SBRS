using Microsoft.AspNetCore.Identity;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Models;

namespace SBRS.Infrastructure.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(
        string userName, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user is null || !user.IsActive)
            return null;

        if (await userManager.IsLockedOutAsync(user))
            return null;

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return await BuildAsync(user);
    }

    public async Task<AuthenticatedUser?> GetAuthenticatedUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
            return null;

        if (await userManager.IsLockedOutAsync(user))
            return null;

        return await BuildAsync(user);
    }

    private async Task<AuthenticatedUser?> BuildAsync(ApplicationUser user)
    {
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
        if (role is null)
            return null;   // a user without a role cannot log in

        return new AuthenticatedUser(
            user.Id, user.UserName!, user.FirstName, user.LastName, role,
            user.OrganizationId, user.FacilityId, user.IsMobile);
    }
}