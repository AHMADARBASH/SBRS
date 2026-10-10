using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Organizations.DTOs;
using SBRS.Application.Features.Organizations.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.Organizations.Commands.UpdateOrganization;

public record UpdateOrganizationCommand(Guid Id, string ArName, string EnName, string Code)
    : IRequest<OrganizationDto>;

public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ArName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EnName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
    }
}

public class UpdateOrganizationCommandHandler(IOrganizationRepository organizations, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateOrganizationCommand, OrganizationDto>
{
    public async Task<OrganizationDto> Handle(UpdateOrganizationCommand request, CancellationToken ct)
    {
        var entity = await organizations.GetByIdAsync(request.Id, ct)
                     ?? throw new NotFoundException(nameof(OrganizationEntity), request.Id);

        var code = request.Code.Trim();

        if (await organizations.CodeExistsAsync(code, request.Id, ct))
            throw new ConflictException($"An organization with code '{code}' already exists.");

        entity.Update(request.ArName, request.EnName, code);
        await unitOfWork.SaveChangesAsync(ct);

        return entity.ToDto();
    }
}