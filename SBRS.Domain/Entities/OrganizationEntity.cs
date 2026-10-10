using SBRS.Domain.Common;
using SBRS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Entities
{
    public class OrganizationEntity : BaseEntity
    {
        public string ArName { get; private set; } = default!;
        public string EnName { get; private set; } = default!;
        public string Code { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;   // needed for soft delete

        private OrganizationEntity() { }   // EF Core

        public OrganizationEntity(string arName, string enName, string code)
        {
            ArName = arName.Trim();
            EnName = enName.Trim();
            Code = code.Trim();
        }

        public void Update(string arName, string enName, string code)
        {
            if (!IsActive)
                throw new DomainException("A deactivated organization cannot be updated.");

            ArName = arName.Trim();
            EnName = enName.Trim();
            Code = code.Trim();
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new DomainException("The organization is already deactivated.");

            IsActive = false;
        }
    }
}
