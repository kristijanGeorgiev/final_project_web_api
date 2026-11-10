using AuthService.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var adminUsername = configuration["BootstrapAdmin:Username"];
        var adminEmail = configuration["BootstrapAdmin:Email"];
        var adminPassword = configuration["BootstrapAdmin:Password"];

        // Do not initialize an admin without explicit bootstrap configuration.
        if (string.IsNullOrWhiteSpace(adminUsername) ||
            string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Bootstrap only when no administrator already exists.
        if ((await userManager.GetUsersInRoleAsync("Admin")).Any())
            return;

        // Do not overwrite or promote an existing account.
        if (await userManager.FindByNameAsync(adminUsername) is not null ||
            await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            throw new InvalidOperationException(
                "Bootstrap administrator identity already exists.");
        }

        var admin = new ApplicationUser
        {
            FullName = "System Administrator",
            UserName = adminUsername,
            Email = adminEmail,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(
            admin,
            adminPassword);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    createResult.Errors.Select(e => e.Description)));
        }

        var roleResult = await userManager.AddToRoleAsync(
            admin,
            "Admin");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(admin);

            throw new InvalidOperationException(
                string.Join("; ",
                    roleResult.Errors.Select(e => e.Description)));
        }
    }
}