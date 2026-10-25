using Microsoft.AspNetCore.Authorization;
using SBRS.Domain.Constants;

namespace SBRS.Authorization;

public static class Policies
{
    public const string SystemAdminOnly = "SystemAdminOnly";
    public const string OrganizationLevel = "OrganizationLevel";   // SystemAdmin + OrganizationAdmin

    public const string branchLevel = "BranchLevel"; // SystemAdmin + OrganizationAdmin + BranchAdmin

    public const string DataEntry = "DataEntry"; // everyone who can register/search patients

    public const string RegisterBeneficiaries = "RegisterBeneficiaries"; // DataEntry only (SystemAdmin has no facility)
    public const string SearchBeneficiaries = "SearchBeneficiaries";     // DataEntry, FacilityAdmin, SystemAdmin
    public const string ReviewBeneficiaryMatches = "ReviewBeneficiaryMatches"; // FacilityAdmin, SystemAdmin
    public const string ViewFacilityBeneficiaries = "ViewFacilityBeneficiaries"; // DataEntry, FacilityAdmin, SystemAdmin

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

        options.AddPolicy(DataEntry, p => p.RequireRole(
            Roles.SystemAdmin, Roles.OrganizationAdmin, Roles.BranchAdmin,
            Roles.ProjectCoordinator, Roles.FacilityAdmin, Roles.DataEntry));

        options.AddPolicy(RegisterBeneficiaries, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(SearchBeneficiaries, p => p.RequireRole(
            Roles.DataEntry, Roles.FacilityAdmin, Roles.SystemAdmin));

        options.AddPolicy(ReviewBeneficiaryMatches, p => p.RequireRole(
            Roles.FacilityAdmin, Roles.SystemAdmin));

        options.AddPolicy(ViewFacilityBeneficiaries, p => p.RequireRole(
            Roles.DataEntry, Roles.FacilityAdmin, Roles.SystemAdmin));

        // More policies are added as modules are built, for example:
        // ManageUsers, ManageClinics, ManageMedicines, EnterPatientData, ViewReports
    }
}