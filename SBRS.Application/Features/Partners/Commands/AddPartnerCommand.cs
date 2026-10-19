using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Partners.DTOs;
using SBRS.Application.Features.Partners.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Partners.Commands;

public sealed record AddPartnerCommand(string ArName, string EnName, string Code) : IRequest<PartnerDto>;

public sealed class AddPartnerCommandValidator : AbstractValidator<AddPartnerCommand>
{
    public AddPartnerCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public sealed class AddPartnerCommandHandler(IPartnerRepository partners, IUnitOfWork uow)
    : IRequestHandler<AddPartnerCommand, PartnerDto>
{
    public async Task<PartnerDto> Handle(AddPartnerCommand c, CancellationToken ct)
    {
        var code = c.Code.Trim();
        if (await partners.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"Partner code '{code}' already exists.");

        var partner = new PartnerEntity(c.ArName, c.EnName, code);
        await partners.AddAsync(partner, ct);
        await uow.SaveChangesAsync(ct);

        return partner.ToDto();
    }
}