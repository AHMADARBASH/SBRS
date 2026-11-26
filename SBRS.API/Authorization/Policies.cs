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
    public const string UpdateBeneficiaryRegistration = "UpdateBeneficiaryRegistration"; // DataEntry only (own facility)
    public const string SearchBeneficiaries = "SearchBeneficiaries";     // DataEntry, FacilityAdmin, SystemAdmin
    public const string ReviewBeneficiaryMatches = "ReviewBeneficiaryMatches"; // FacilityAdmin, SystemAdmin
    public const string ViewFacilityBeneficiaries = "ViewFacilityBeneficiaries"; // DataEntry, FacilityAdmin, SystemAdmin
    public const string EnrollBeneficiaries = "EnrollBeneficiaries";           // DataEntry, FacilityAdmin (enroll at own facility)
    public const string DeactivateBeneficiaries = "DeactivateBeneficiaries";   // DataEntry, FacilityAdmin, SystemAdmin (soft delete)
    public const string ManageBeneficiaryProfiles = "ManageBeneficiaryProfiles"; // DataEntry, FacilityAdmin, SystemAdmin (profile create/update; reads also allow the trio)
    public const string ManageClinicTypes = "ManageClinicTypes";       // SystemAdmin
    public const string ManageClinics = "ManageClinics";               // SystemAdmin, FacilityAdmin
    public const string ManageDoctors = "ManageDoctors";               // SystemAdmin, FacilityAdmin
    public const string ViewClinics = "ViewClinics";                   // SystemAdmin, FacilityAdmin, DataEntry
    public const string ViewDoctors = "ViewDoctors";                   // SystemAdmin, FacilityAdmin, DataEntry
    public const string RecordVisits = "RecordVisits";                 // DataEntry only
    public const string UpdateVisits = "UpdateVisits";                 // DataEntry only
    public const string DeleteVisits = "DeleteVisits";                 // DataEntry only
    public const string ViewVisits = "ViewVisits";                     // DataEntry only
    public const string RecordConsultations = "RecordConsultations";   // DataEntry only
    public const string UpdateConsultations = "UpdateConsultations";   // DataEntry only
    public const string DeleteConsultations = "DeleteConsultations";   // DataEntry only
    public const string ViewConsultations = "ViewConsultations";       // DataEntry only
    public const string ManageDiseaseCategories = "ManageDiseaseCategories"; // SystemAdmin
    public const string ManageDiseases = "ManageDiseases";             // SystemAdmin

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

        options.AddPolicy(UpdateBeneficiaryRegistration, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(SearchBeneficiaries, p => p.RequireRole(
            Roles.DataEntry, Roles.FacilityAdmin, Roles.SystemAdmin));

        options.AddPolicy(ReviewBeneficiaryMatches, p => p.RequireRole(
            Roles.FacilityAdmin, Roles.SystemAdmin));

        options.AddPolicy(ViewFacilityBeneficiaries, p => p.RequireRole(
            Roles.DataEntry, Roles.FacilityAdmin, Roles.SystemAdmin));

        // DataEntry and FacilityAdmin enroll at their own facility. SystemAdmin has no facility
        // claim. Pharmacy/report roles do not exist yet; when added they must not be included.
        options.AddPolicy(EnrollBeneficiaries, p => p.RequireRole(
            Roles.DataEntry, Roles.FacilityAdmin));

        options.AddPolicy(DeactivateBeneficiaries, p => p.RequireRole(
            Roles.DataEntry, Roles.FacilityAdmin, Roles.SystemAdmin));

        options.AddPolicy(ManageBeneficiaryProfiles, p => p.RequireRole(
            Roles.DataEntry, Roles.FacilityAdmin, Roles.SystemAdmin));

        options.AddPolicy(ManageClinicTypes, p => p.RequireRole(Roles.SystemAdmin));

        options.AddPolicy(ManageClinics, p => p.RequireRole(Roles.SystemAdmin, Roles.FacilityAdmin));

        options.AddPolicy(ManageDoctors, p => p.RequireRole(Roles.SystemAdmin, Roles.FacilityAdmin));

        options.AddPolicy(ViewClinics, p => p.RequireRole(
            Roles.SystemAdmin, Roles.FacilityAdmin, Roles.DataEntry));

        options.AddPolicy(ViewDoctors, p => p.RequireRole(
            Roles.SystemAdmin, Roles.FacilityAdmin, Roles.DataEntry));

        options.AddPolicy(RecordVisits, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(UpdateVisits, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(DeleteVisits, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(ViewVisits, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(RecordConsultations, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(UpdateConsultations, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(DeleteConsultations, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(ViewConsultations, p => p.RequireRole(Roles.DataEntry));

        options.AddPolicy(ManageDiseaseCategories, p => p.RequireRole(Roles.SystemAdmin));

        options.AddPolicy(ManageDiseases, p => p.RequireRole(Roles.SystemAdmin));

        // More policies are added as modules are built, for example:
        // ManageUsers, ManageClinics, ManageMedicines, EnterPatientData, ViewReports
    }
}