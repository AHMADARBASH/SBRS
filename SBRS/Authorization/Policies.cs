using Microsoft.AspNetCore.Authorization;
using SBRS.Domain.Constants;

namespace SBRS.Authorization;

public static class Policies
{
    public const string SystemAdminOnly = "SystemAdminOnly";
    public const string OrganizationLevel = "OrganizationLevel";   // SystemAdmin + OrganizationAdmin

    public const string branchLevel = "BranchLevel"; // SystemAdmin + OrganizationAdmin + BranchAdmin

    public static void Configure(AuthorizationOptions options)
    {
        // Safe default: every endpoint needs a logged-in user unless it says [AllowAnonymous]
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.AddPolicy(SystemAdminOnly, p => p.RequireRole(Roles.SystemAdmin));

        options.AddPolicy(OrganizationLevel, p => p.RequireRole(
            Roles.SystemAdmin, Roles.OrganizationAdmin));

        options.AddPolicy(branchLevel, p => p.RequireRole(
            Roles.SystemAdmin, Roles.OrganizationAdmin, Roles.BranchAdmin));

        // More policies are added as modules are built, for example:
        // ManageUsers, ManageClinics, ManageMedicines, EnterPatientData, ViewReports
    }
}