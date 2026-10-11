using SBRS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Entities
{
    public class FacilityEntity :BaseEntity
    {
        public string ArName { get; set; } = null!;
        public string EnName { get; set; } = null!; 
        public Guid OrganizationId { get; set; }



        //Navigation property to the related OrganizationEntity
        public OrganizationEntity Organization { get; set; } = null!;


    }
}
