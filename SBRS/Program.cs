using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SBRS;
using SBRS.Infrastructure.Persistence;
using SBRS.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
///testing the pipeline  1

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAppDI(builder.Configuration);

// CORS: origins come from configuration; never AllowAnyOrigin together with credentials.
var origins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    policy.AllowAnyHeader().AllowAnyMethod();

    if (origins.Length == 0 || origins.Contains("*"))
        policy.SetIsOriginAllowed(_ => true).AllowCredentials();
    else
        policy.WithOrigins(origins).AllowCredentials();
}));  // required so browsers keep the refresh cookie; only with a configured origin list

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Schema first: the production database is migrated by the application itself, no manual SQL.
    if (scope.ServiceProvider.GetRequiredService<IConfiguration>()
            .GetValue("Database:ApplyMigrationsOnStartup", true))
    {
        try
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed; the application cannot start safely.");
            throw;   // a half-migrated schema is worse than downtime
        }
    }

    // Identity bootstrap (roles + seed admin), then the idempotent reference data.
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<ReferenceDataSeeder>().SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SBRS API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseExceptionHandler();

app.UseHttpsRedirection();

// after routing (useRouting runs implicitly), before authentication/authorization
app.UseCors();

app.UseAuthentication();  

app.UseAuthorization();

app.MapControllers();

app.Run();
