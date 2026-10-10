using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Domain.Common
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; } 
    }
}
