using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Models;

public record AuthenticatedUser(
Guid Id,
string UserName,
string FirstName,
string LastName,
string Role,
Guid? OrganizationId,
Guid? BranchId,
Guid? FacilityId,
bool IsMobile);
