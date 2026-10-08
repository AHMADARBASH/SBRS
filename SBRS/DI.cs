using SBRS.Application;
using SBRS.Infrastructure;

namespace SBRS
{
    public static class DI
    {
        public static IServiceCollection AddAppDI(this IServiceCollection services)
        {
            services.AddApplicationDI()
                    .AddInfrastructureDI();
            return services;
        }
    }
}
