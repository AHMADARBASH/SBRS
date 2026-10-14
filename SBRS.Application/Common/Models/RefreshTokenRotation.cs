using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Models
{
    public record RefreshTokenRotation(Guid UserId, string RefreshToken);
}
