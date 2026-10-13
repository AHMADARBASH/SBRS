using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Interfaces
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
        string? Role { get; }
        Guid? OrganizationId { get; }
        Guid? FacilityId { get; }
        bool IsSystemAdmin { get; }
    }
}
