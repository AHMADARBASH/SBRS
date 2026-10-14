using SBRS.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthenticatedUser?> ValidateCredentialsAsync(
   string userName, string password, CancellationToken ct = default);
    Task<AuthenticatedUser?> GetAuthenticatedUserAsync(Guid userId, CancellationToken ct = default);
}
