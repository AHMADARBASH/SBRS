using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Exceptions
{
    public class NotFoundException(string name, object key)
    : Exception($"{name} with id '{key}' was not found.");
}
