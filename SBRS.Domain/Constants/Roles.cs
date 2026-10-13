

namespace SBRS.Domain.Constants;
public static class Roles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string OrganizationAdmin = "OrganizationAdmin";
    public const string FacilityAdmin = "FacilityAdmin";
    public const string DataEntry = "DataEntry";
    public const string Reporter = "Reporter";

    public static readonly string[] All =
        [SystemAdmin, OrganizationAdmin, FacilityAdmin, DataEntry, Reporter];
}