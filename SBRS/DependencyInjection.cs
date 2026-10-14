using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SBRS.Application;
using SBRS.Application.Common.Interfaces;
using SBRS.Application.Common.Settings;
using SBRS.Authorization;
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

            services.AddAuthorization(Policies.Configure);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SBRS API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the access token only, without the 'Bearer ' prefix."
                });

                // new in Swashbuckle 10: the requirement is a callback that receives the document
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            return services;
        }
    }
}
