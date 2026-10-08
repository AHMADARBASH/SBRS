using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SBRS.Domain.Interfaces;
using SBRS.Infrastructure.Data;
using SBRS.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Infrastructure
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructureDI (this IServiceCollection services)
        {
            services.AddDbContext<SBRSDbContext>(options =>
            options.UseSqlServer("Server=.;Database=SBRS;Trusted_Connection=true;trustservercertificate=true;MultipleActiveResultSets=true")
            );
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            return services;
        }
    }
}
