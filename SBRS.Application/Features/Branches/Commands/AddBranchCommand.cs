using FluentValidation;
using MediatR;
using SBRS.Application.Features.Branches.DTOs;
using SBRS.Application.Features.Branches.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;


namespace SBRS.Application.Features.Branches.Commands
{
    public record AddBranchCommand(Guid OrganizationId,string ArName, string EnName) : IRequest<BranchDto>;

    public class AddBranchCommandValidator : AbstractValidator<AddBranchCommand>
    {
        public AddBranchCommandValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.ArName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.EnName).NotEmpty().MaximumLength(50);
        }
    }


    public class AddBranchCommandHandler(IBranchRepository branches,IUnitOfWork unitOfWork) : IRequestHandler<AddBranchCommand, BranchDto>
    {
        public async Task<BranchDto> Handle(AddBranchCommand request, CancellationToken ct)
        {
            
            var entity = new BranchEntity(request.OrganizationId,request.ArName,request.EnName, true);
            await branches.AddAsync(entity, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return entity.ToDto();
        }
    }

}
