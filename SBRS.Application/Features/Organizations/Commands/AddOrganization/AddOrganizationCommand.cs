using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Organizations.DTOs;
using SBRS.Application.Features.Organizations.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Organizations.Commands.AddOrganization;

public record AddOrganizationCommand(string ArName, string EnName, string Code)
    : IRequest<OrganizationDto>;

public class AddOrganizationCommandValidator : AbstractValidator<AddOrganizationCommand>
{
    public AddOrganizationCommandValidator()
    {
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public class AddOrganizationCommandHandler(IOrganizationRepository organizations, IUnitOfWork unitOfWork)
    : IRequestHandler<AddOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(AddOrganizationCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim();

        if (await organizations.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"An organization with code '{code}' already exists.");

        var entity = new OrganizationEntity(request.ArName, request.EnName, code);

        await organizations.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}