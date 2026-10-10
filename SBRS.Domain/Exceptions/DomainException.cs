using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Exceptions
{
    public class DomainException(string message) : Exception(message);
}
