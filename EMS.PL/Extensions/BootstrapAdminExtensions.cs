using System.Security.Claims;
using EMS.DAL.Entities.IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace EMS.PL.Extensions;

public static class BootstrapAdminExtensions
{
    private const string AdminRole = "Admin";
    private const string SeedMarkerClaim = "ems.bootstrap-admin-seeded";

    public static async Task SeedBootstrapAdminAsync(this WebApplication app)
    {
        var email = app.Configuration["BootstrapAdmin:Email"]?.Trim();
        var password = app.Configuration["BootstrapAdmin:Password"];

        // The initial administrator is opt-in and supplied through host environment
        // variables. Never keep account credentials in the source repository.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return;

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRole));
            if (!roleResult.Succeeded)
            {
                logger.LogError("Could not create the bootstrap Admin role: {Errors}",
                    string.Join("; ", roleResult.Errors.Select(error => error.Code)));
                return;
            }
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "Admin",
                LastName = "Account"
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                logger.LogError("Could not create the bootstrap Admin user for {Email}: {Errors}", email,
                    string.Join("; ", createResult.Errors.Select(error => error.Code)));
                return;
            }
        }

        var marker = (await userManager.GetClaimsAsync(user))
            .Any(claim => claim.Type == SeedMarkerClaim && claim.Value == "true");
        if (marker)
            return;

        if (!await userManager.IsInRoleAsync(user, AdminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(user, AdminRole);
            if (!roleResult.Succeeded)
            {
                logger.LogError("Could not assign the bootstrap Admin role to {Email}: {Errors}", email,
                    string.Join("; ", roleResult.Errors.Select(error => error.Code)));
                return;
            }
        }

        var markerResult = await userManager.AddClaimAsync(user, new Claim(SeedMarkerClaim, "true"));
        if (!markerResult.Succeeded)
        {
            logger.LogWarning("The bootstrap Admin was assigned to {Email}, but its one-time seed marker could not be saved: {Errors}",
                email, string.Join("; ", markerResult.Errors.Select(error => error.Code)));
            return;
        }

        logger.LogInformation("Bootstrap Admin setup completed for {Email}.", email);
    }
}
