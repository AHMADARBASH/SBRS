using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SBRS.Application.Common.Behaviors;
using SBRS.Application.Features.Beneficiaries.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBRS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            services.AddMediatR(cfg => 
            { 
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));    
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            services.AddScoped<IBeneficiaryMatchService, BeneficiaryMatchService>();
            return services;
        }
    }
}
