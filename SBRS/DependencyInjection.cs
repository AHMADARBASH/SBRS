using SBRS.Application;
using SBRS.Infrastructure;
using SBRS.Middleware;

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
            return services;
        }
    }
}
