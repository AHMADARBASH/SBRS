using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Partners.DTOs;
using SBRS.Application.Features.Partners.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Partners.Commands;

public sealed record UpdatePartnerCommand(Guid Id, string ArName, string EnName, string Code) : IRequest<PartnerDto>;

public sealed class UpdatePartnerCommandValidator : AbstractValidator<UpdatePartnerCommand>
{
    public UpdatePartnerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public sealed class UpdatePartnerCommandHandler(IPartnerRepository partners, IUnitOfWork uow)
    : IRequestHandler<UpdatePartnerCommand, PartnerDto>
{
    public async Task<PartnerDto> Handle(UpdatePartnerCommand c, CancellationToken ct)
    {
        var partner = await partners.GetByIdAsync(c.Id, ct)
                      ?? throw new NotFoundException(nameof(PartnerEntity),c.Id);

        var code = c.Code.Trim();
        if (await partners.CodeExistsAsync(code, c.Id, ct))
            throw new ConflictException($"Partner code '{code}' already exists.");

        partner.Update(c.ArName, c.EnName, code);
        await uow.SaveChangesAsync(ct);

        return partner.ToDto();
    }
}