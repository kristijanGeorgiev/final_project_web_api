
using AuthService.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var roleManager =
            serviceProvider.GetRequiredService<
                RoleManager<IdentityRole>>();

        var userManager =
            serviceProvider.GetRequiredService<
                UserManager<ApplicationUser>>();

        var environment =
            serviceProvider.GetRequiredService<
                IHostEnvironment>();

        var logger =
            serviceProvider.GetRequiredService<
                ILoggerFactory>()
            .CreateLogger("IdentitySeeder");

        // =====================================================
        // CREATE ROLES
        // =====================================================

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role {role}: " +
                    string.Join("; ",
                        result.Errors.Select(
                            e => e.Description)));
            }
        }

        // =====================================================
        // READ INITIAL ADMIN CONFIGURATION
        // =====================================================

        var username =
            configuration["SeedAdmin:Username"]?.Trim();

        var email =
            configuration["SeedAdmin:Email"]?.Trim();

        var password =
            configuration["SeedAdmin:Password"];

        var unlockRequested =
            configuration.GetValue<bool>(
                "SeedAdmin:UnlockOnStartup");

        var resetPasswordRequested =
            configuration.GetValue<bool>(
                "SeedAdmin:ResetPasswordOnStartup");

        // =====================================================
        // VALIDATE RECOVERY ENVIRONMENT
        // =====================================================

        if ((unlockRequested || resetPasswordRequested) &&
            !environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Administrator recovery operations are only " +
                "permitted in the Development environment.");
        }

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            if (unlockRequested || resetPasswordRequested)
            {
                throw new InvalidOperationException(
                    "Administrator recovery requires a " +
                    "complete SeedAdmin configuration.");
            }

            logger.LogWarning(
                "Initial administrator configuration is " +
                "incomplete. Administrator seeding skipped.");

            return;
        }

        // =====================================================
        // FIND EXISTING ADMINISTRATOR
        // =====================================================

        var admin =
            await userManager.FindByNameAsync(username);

        if (admin is null)
        {
            var existingEmailUser =
                await userManager.FindByEmailAsync(email);

            if (existingEmailUser is not null)
            {
                throw new InvalidOperationException(
                    "The configured administrator email belongs " +
                    "to a different username.");
            }

            // =================================================
            // CREATE INITIAL ADMINISTRATOR
            // =================================================

            admin = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FullName = "System Administrator",
                CreatedAt = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(
                    admin,
                    password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create initial administrator: " +
                    string.Join("; ",
                        createResult.Errors.Select(
                            e => e.Description)));
            }

            logger.LogInformation(
                "Initial administrator created successfully.");
        }
        else
        {
            logger.LogInformation(
                "Initial administrator already exists. " +
                "Existing password has not been modified.");

            if (!string.Equals(
                    admin.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "Configured administrator email differs " +
                    "from the existing account.");
            }
        }

        // =====================================================
        // ASSIGN ADMIN ROLE
        // =====================================================

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    admin,
                    "Admin");

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign Admin role: " +
                    string.Join("; ",
                        roleResult.Errors.Select(
                            e => e.Description)));
            }
        }

        // =====================================================
        // DEVELOPMENT-ONLY PASSWORD RESET
        // =====================================================

        if (resetPasswordRequested)
        {
            var resetToken =
                await userManager
                    .GeneratePasswordResetTokenAsync(admin);

            var resetResult =
                await userManager.ResetPasswordAsync(
                    admin,
                    resetToken,
                    password);

            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to reset administrator password: " +
                    string.Join("; ",
                        resetResult.Errors.Select(
                            e => e.Description)));
            }

            logger.LogWarning(
                "Administrator password reset through " +
                "explicit development recovery configuration.");
        }

        // =====================================================
        // DEVELOPMENT-ONLY ACCOUNT UNLOCK
        // =====================================================

        if (unlockRequested)
        {
            var unlockResult =
                await userManager.SetLockoutEndDateAsync(
                    admin,
                    null);

            if (!unlockResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to unlock administrator: " +
                    string.Join("; ",
                        unlockResult.Errors.Select(
                            e => e.Description)));
            }

            var resetAttemptsResult =
                await userManager
                    .ResetAccessFailedCountAsync(admin);

            if (!resetAttemptsResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to reset administrator failed " +
                    "login attempts: " +
                    string.Join("; ",
                        resetAttemptsResult.Errors.Select(
                            e => e.Description)));
            }

            logger.LogWarning(
                "Administrator account unlocked through " +
                "explicit development recovery configuration.");
        }

        // =====================================================
        // COMPLETE
        // =====================================================

        logger.LogInformation(
            "Identity roles and administrator seeding completed.");
    }
}