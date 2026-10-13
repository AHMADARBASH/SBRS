using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public Guid? OrganizationId { get; set; }
        public Guid? FacilityId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsMobile { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}
