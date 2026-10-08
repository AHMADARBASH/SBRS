using MediatR;
using SBRS.Domain.Entities;
using SBRS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Commands
{
    public record AddOrganizationCommand(OrganizationEntity organization) : IRequest<OrganizationEntity>;
    

    public class AddOrganizationCommandHandler(IOrganizationRepository organizationRepository):
        IRequestHandler<AddOrganizationCommand, OrganizationEntity>
    {
       

        public async Task<OrganizationEntity> Handle(AddOrganizationCommand request, CancellationToken cancellationToken)
        {
            return await organizationRepository.AddOrganization(request.organization);
        }
    }
}
