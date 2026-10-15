using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;


namespace SBRS.Application.Features.Branches.Commands
{
    public record UpdateBranchCommand(Guid Id,string ArName, string EnName) : IRequest<BranchDto>;


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
            entity.update(request.ArName, request.EnName);
            await unitOfWork.SaveChangesAsync(ct);
            return entity.ToDto();
        }
    }
}
