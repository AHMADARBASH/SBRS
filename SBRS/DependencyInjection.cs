using SBRS.Application;
using SBRS.Application.Common.Interfaces;
using SBRS.Infrastructure;
using SBRS.Middleware;
using SBRS.Services;

namespace SBRS
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppDI(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddApplicationDI()
                    .AddInfrastructureDI(configuration);
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            return services;
        }
    }
}
