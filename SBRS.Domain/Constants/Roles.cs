namespace SBRS.Domain.Constants;

public static class Roles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string OrganizationAdmin = "OrganizationAdmin";
    public const string BranchAdmin = "BranchAdmin";
    public const string ProjectCoordinator = "ProjectCoordinator";
    public const string FacilityAdmin = "FacilityAdmin";
    public const string DataEntry = "DataEntry";

    public static readonly string[] All =
    [
        SystemAdmin, OrganizationAdmin, BranchAdmin,
        ProjectCoordinator, FacilityAdmin, DataEntry
    ];
}