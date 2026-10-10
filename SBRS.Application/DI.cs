using Microsoft.Extensions.DependencyInjection;
using SBRS.Application.Common.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application
{
    public static class DI
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            services.AddMediatR(cfg => 
            { 
                cfg.RegisterServicesFromAssembly(typeof(DI).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            return services;
        }
    }
}
