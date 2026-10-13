using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SBRS.Domain.Constants;
using SBRS.Infrastructure.Identity;

namespace SBRS.Infrastructure.Persistence.Seed;

public class IdentitySeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync()
    {
        foreach (var role in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role) { Id = Guid.NewGuid() });
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create role '{role}': " + string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        var userName = configuration["SeedAdmin:UserName"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("SeedAdmin credentials are not configured; skipping first admin creation.");
            return;
        }

        if (await userManager.FindByNameAsync(userName) is not null)
            return;   // admin already exists

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Failed to create the seed admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(admin, Roles.SystemAdmin);
        logger.LogInformation("Seed SystemAdmin created: {UserName}", userName);
    }
}