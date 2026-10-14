using SBRS.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Interfaces
{
    public interface ITokenService
    {
        TokenResult CreateToken(AuthenticatedUser user);
    }
}
