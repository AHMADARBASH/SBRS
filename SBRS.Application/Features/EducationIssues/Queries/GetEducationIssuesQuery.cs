using MediatR;
using SBRS.Application.Features.EducationIssues.DTOs;
using SBRS.Application.Features.EducationIssues.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.EducationIssues.Queries;

public sealed record GetEducationIssuesQuery : IRequest<IReadOnlyList<EducationIssueDto>>;

public sealed class GetEducationIssuesQueryHandler(IEducationIssueRepository issues)
    : IRequestHandler<GetEducationIssuesQuery, IReadOnlyList<EducationIssueDto>>
{
    public async Task<IReadOnlyList<EducationIssueDto>> Handle(GetEducationIssuesQuery q,
        CancellationToken ct) => (await issues.GetAllAsync(ct)).Select(i => i.ToDto()).ToList();
}
