using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Exceptions
{
    public class ConflictException(string message) : Exception(message);
}
