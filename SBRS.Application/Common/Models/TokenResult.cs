using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application.Common.Models;

public record TokenResult(string Token, DateTime ExpiresAt);
