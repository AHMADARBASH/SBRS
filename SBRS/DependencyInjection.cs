using SBRS.Application;
using SBRS.Infrastructure;

namespace SBRS
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppDI(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddApplicationDI()
                    .AddInfrastructureDI(configuration);
            return services;
        }
    }
}
