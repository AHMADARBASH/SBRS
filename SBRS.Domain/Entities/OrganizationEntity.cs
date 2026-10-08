using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Entities
{
    public class OrganizationEntity
    {
        public Guid Id { get; set; }
        public required string ArName { get; set; }
        public required string EnName { get; set; } 


    }
}
