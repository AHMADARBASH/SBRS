using SBRS;
using SBRS.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAppDI(builder.Configuration);

// CORS: origins come from configuration; never AllowAnyOrigin together with credentials.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()        // includes X-Client-Type (forces the preflight) and Authorization
    .AllowAnyMethod()
    .AllowCredentials()));   // required so browsers keep the refresh cookie; only with a configured origin list

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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

app.UseExceptionHandler();

app.UseHttpsRedirection();

// after routing (useRouting runs implicitly), before authentication/authorization
app.UseCors();

app.UseAuthentication();  

app.UseAuthorization();

app.MapControllers();

app.Run();
