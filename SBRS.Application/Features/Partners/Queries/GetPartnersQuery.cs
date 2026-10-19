using MediatR;
using SBRS.Application.Features.Partners.DTOs;
using SBRS.Application.Features.Partners.Mappings;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Partners.Queries;

public sealed record GetPartnersQuery : IRequest<IReadOnlyList<PartnerDto>>;

public sealed class GetPartnersQueryHandler(IPartnerRepository partners)
    : IRequestHandler<GetPartnersQuery, IReadOnlyList<PartnerDto>>
{
    public async Task<IReadOnlyList<PartnerDto>> Handle(GetPartnersQuery q, CancellationToken ct) =>
        (await partners.GetAllAsync(ct)).Select(p => p.ToDto()).ToList();
}