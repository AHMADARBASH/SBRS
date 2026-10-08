using Microsoft.EntityFrameworkCore;
using SBRS.Domain.Entities;

namespace SBRS.Infrastructure.Data
{
    public class SBRSDbContext(DbContextOptions<SBRSDbContext> options) :DbContext(options)
    {
        public DbSet<OrganizationEntity> Organizations { get; set; }
    }
}
