using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SBRS.Domain.Interfaces;
using SBRS.Infrastructure.Identity;
using SBRS.Infrastructure.Persistence;
using SBRS.Infrastructure.Persistence.Repositories;
using SBRS.Infrastructure.Persistence.Seed;

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
        services.AddIdentityCore<ApplicationUser>(
            options => {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}