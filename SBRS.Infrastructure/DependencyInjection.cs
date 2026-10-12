using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SBRS.Domain.Interfaces;
using SBRS.Infrastructure.Persistence;
using SBRS.Infrastructure.Persistence.Repositories;

namespace SBRS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDI(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // same scoped AppDbContext instance for the repository and the unit of work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IGeographyRepository, GeographyRepository>();

        return services;
    }
}