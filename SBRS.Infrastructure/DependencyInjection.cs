using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SBRS.Application.Common.Interfaces;
using SBRS.Domain.Interfaces;
using SBRS.Infrastructure.BackgroundJobs;
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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<AppDbContext>(options =>
     options
         .UseNpgsql(
             connectionString,
             npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
         .UseSnakeCaseNamingConvention());

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
            .AddEntityFrameworkStores<AppDbContext>()
            // AddIdentityCore does not register token providers; without this every
            // GeneratePasswordResetTokenAsync call throws NotSupportedException (HTTP 500).
            .AddDefaultTokenProviders();
        services.AddScoped<IdentitySeeder>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Expired refresh tokens are pruned by a hosted service (section "RefreshTokenCleanup").
        services.Configure<RefreshTokenCleanupSettings>(
            configuration.GetSection(RefreshTokenCleanupSettings.SectionName));
        services.AddHostedService<RefreshTokenCleanupService>();

        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IFacilityTypeRepository, FacilityTypeRepository>();
services.AddScoped<IFacilityCategoryRepository, FacilityCategoryRepository>();
        services.AddScoped<INationalityRepository, NationalityRepository>();
        services.AddScoped<IIdentityTypeRepository, IdentityTypeRepository>();
        services.AddScoped<IAccommodationTypeRepository, AccommodationTypeRepository>();
        services.AddScoped<IDisabilityTypeRepository, DisabilityTypeRepository>();
        services.AddScoped<IEducationTypeRepository, EducationTypeRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IFacilityBeneficiaryRepository, FacilityBeneficiaryRepository>();
        services.AddScoped<IBeneficiaryIdentityRepository, BeneficiaryIdentityRepository>();
        services.AddScoped<IBeneficiaryProfileRepository, BeneficiaryProfileRepository>();
        services.AddScoped<IBeneficiaryStatusRepository, BeneficiaryStatusRepository>();
        services.AddScoped<IMaritalStatusRepository, MaritalStatusRepository>();
        services.AddScoped<IDisabilityReasonRepository, DisabilityReasonRepository>();
        services.AddScoped<IChronicDiseaseRepository, ChronicDiseaseRepository>();
        services.AddScoped<IEducationIssueRepository, EducationIssueRepository>();
        services.AddScoped<OrganizationSeeder>();
        services.AddScoped<ReferenceDataSeeder>();
        services.AddScoped<BranchSeeder>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IFacilityPartnerRepository, FacilityPartnerRepository>();
        services.AddScoped<IClinicTypeRepository, ClinicTypeRepository>();
        services.AddScoped<IClinicRepository, ClinicRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IClinicDoctorRepository, ClinicDoctorRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<IBeneficiaryConsultationRepository, BeneficiaryConsultationRepository>();
        services.AddScoped<IDiseaseCategoryRepository, DiseaseCategoryRepository>();
        services.AddScoped<IDiseaseRepository, DiseaseRepository>();
        services.AddScoped<DiseaseSeeder>();
        services.AddScoped<IAddressTypeRepository, AddressTypeRepository>();
        services.AddScoped<IWorkingAddressRepository, WorkingAddressRepository>();
        services.AddScoped<ClinicTypeSeeder>();

        return services;
    }
}