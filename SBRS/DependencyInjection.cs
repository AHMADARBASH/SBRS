using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SBRS.Application;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Settings;
using SBRS.Infrastructure;
using SBRS.Middleware;
using SBRS.Services;
using System.Text;

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
            var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
             ?? throw new InvalidOperationException("The 'Jwt' settings section is missing.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidIssuer = jwt.Issuer,
               ValidateAudience = true,
               ValidAudience = jwt.Audience,
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
               ValidateLifetime = true,
               ClockSkew = TimeSpan.Zero
           };
       });

            services.AddAuthorization();
            return services;
        }
    }
}
