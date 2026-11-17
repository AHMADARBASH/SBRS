namespace SBRS.Contracts.Branches
{
    public record AddBranchRequest(string ArName, string EnName, string Code, Guid OrganizationId);
}
