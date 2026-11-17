using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;


namespace SBRS.Application.Features.Branches.Commands
{
    public record AddBranchCommand(Guid OrganizationId,string ArName, string EnName, string Code) : IRequest<BranchDto>;

    public class AddBranchCommandValidator : AbstractValidator<AddBranchCommand>
    {
        public AddBranchCommandValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.ArName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.EnName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Branch code is required.")
                .Must(c => c is not null && c.Trim().Length is >= 2 and <= 10)
                .WithMessage("Branch code must be 2 to 10 characters.")
                .Must(c => c is not null && c.Trim().All(char.IsLetter))
                .WithMessage("Branch code must contain letters only.");
        }
    }


    public class AddBranchCommandHandler(IBranchRepository branches,IUnitOfWork unitOfWork) : IRequestHandler<AddBranchCommand, BranchDto>
    {
        public async Task<BranchDto> Handle(AddBranchCommand request, CancellationToken ct)
        {
            var code = request.Code.Trim().ToUpperInvariant();

            if (await branches.CodeExistsAsync(request.OrganizationId, code, null, ct))
                throw new ConflictException($"A branch with code '{code}' already exists in this organization.");

            var entity = new BranchEntity(request.OrganizationId,request.ArName,request.EnName, code, true);
            await branches.AddAsync(entity, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return entity.ToDto();
        }
    }

}
