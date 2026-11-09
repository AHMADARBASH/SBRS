using SBRS.Application.Features.EducationIssues.DTOs;
using SBRS.Domain.Entities;

namespace SBRS.Application.Features.EducationIssues.Mappings;

public static class EducationIssueMappings
{
    public static EducationIssueDto ToDto(this EducationIssueEntity issue) =>
        new(issue.Id, issue.Code, issue.ArName, issue.EnName);
}
