using Microsoft.AspNetCore.Identity;
using StockPilot.Api.Models;

namespace StockPilot.Api.Data.Seeding;

public static class PlatformAdminBootstrapper
{
    public const string RoleName = "PlatformAdmin";

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(RoleName))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<Guid>(RoleName));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create PlatformAdmin role: " +
                    string.Join("; ", roleResult.Errors.Select(x => x.Description)));
            }
        }

        var existingAdmins = await userManager.GetUsersInRoleAsync(RoleName);

        if (existingAdmins.Count > 0)
            return;

        var email = configuration["PlatformAdmin:Email"];
        var password = configuration["PlatformAdmin:Password"];
        var fullName = configuration["PlatformAdmin:FullName"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName))
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "The configured Platform Admin email already belongs to another account. " +
                "Bootstrap was stopped to prevent accidental privilege escalation.");
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(admin, password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to create Platform Admin: " +
                string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }

        var addRoleResult = await userManager.AddToRoleAsync(
            admin,
            RoleName);

        if (!addRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(admin);

            throw new InvalidOperationException(
                "Failed to assign PlatformAdmin role: " +
                string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
        }
    }
}