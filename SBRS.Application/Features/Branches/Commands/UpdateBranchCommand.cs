using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;


namespace SBRS.Application.Features.Branches.Commands
{
    public record UpdateBranchCommand(Guid Id,string ArName, string EnName, string Code) : IRequest<BranchDto>;


    public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
    {
        public UpdateBranchCommandValidator()
        {
            RuleFor(x => x.ArName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.EnName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Branch code is required.")
                .Must(c => c is not null && c.Trim().Length is >= 2 and <= 10)
                .WithMessage("Branch code must be 2 to 10 characters.")
                .Must(c => c is not null && c.Trim().All(char.IsLetter))
                .WithMessage("Branch code must contain letters only.");
        }
    }

    public class UpdateBranchCommandHandler(IBranchRepository branches, IUnitOfWork unitOfWork) : IRequestHandler<UpdateBranchCommand, BranchDto>
    {
        public async Task<BranchDto> Handle(UpdateBranchCommand request, CancellationToken ct)
        {
            var entity = await branches.GetByIdAsync(request.Id, ct);
            if (entity == null)
            {
                throw new NotFoundException(nameof(BranchEntity), request.Id);
            }

            var code = request.Code.Trim().ToUpperInvariant();

            if (await branches.CodeExistsAsync(entity.organizationId, code, entity.Id, ct))
                throw new ConflictException($"A branch with code '{code}' already exists in this organization.");

            entity.update(request.ArName, request.EnName, code);
            await unitOfWork.SaveChangesAsync(ct);
            return entity.ToDto();
        }
    }
}
