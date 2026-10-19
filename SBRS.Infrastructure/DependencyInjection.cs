using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SBRS.Application.Common.Interfaces;
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
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IFacilityTypeRepository, FacilityTypeRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IFacilityPartnerRepository, FacilityPartnerRepository>();

        return services;
    }
}