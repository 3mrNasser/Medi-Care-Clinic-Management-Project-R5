using MediCare.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Data.Seed;

/// <summary>
/// Startup seeding for hosted environments (Production / Staging). Unlike <see cref="IdentitySeeder"/>,
/// it never creates test data or accounts with a known password. Safe to run on every start.
/// </summary>
public static class ProductionSeeder
{
    /// <summary>
    /// Creates the Patient / Doctor / Admin roles if they are missing, and, when an email and password
    /// are given (App Service settings <c>SeedAdmin__Email</c> and <c>SeedAdmin__Password</c>), makes sure
    /// that user exists and is an Admin. An existing user's password is never changed.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, string? adminEmail, string? adminPassword)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in IdentitySeeder.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                Check(await roleManager.CreateAsync(new IdentityRole(role)));
        }

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Administrator"
            };
            Check(await userManager.CreateAsync(admin, adminPassword));
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            Check(await userManager.AddToRoleAsync(admin, "Admin"));
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Production seeding failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
