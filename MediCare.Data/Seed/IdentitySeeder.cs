using MediCare.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Data.Seed;

// DEVELOPMENT ONLY. Never call this outside the Development environment:
// it creates an admin account and accounts with a known password.
// Safe to run many times: it skips anything that already exists.
public static class IdentitySeeder
{
    public static readonly string[] Roles = { "Patient", "Doctor", "Admin" };

    public const string DevPassword = "Dev123456";
    public const string AdminEmail = "admin@medicare.test";

    private static readonly string[] DoctorEmails =
    {
        "ahmed.hassan@medicare.test",
        "sara.mostafa@medicare.test",
        "omar.khaled@medicare.test",
        "mona.adel@medicare.test"
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Roles
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                Check(await roleManager.CreateAsync(new IdentityRole(role)));
        }

        // 2. Dev admin
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = "Dev Admin"
            };
            Check(await userManager.CreateAsync(admin, DevPassword));
        }
        await EnsurePasswordAndRoleAsync(userManager, admin, "Admin");

        // 3. Seeded test patient (created by DbSeeder)
        var patient = await userManager.FindByIdAsync(DbSeeder.TestPatientId);
        if (patient != null)
            await EnsurePasswordAndRoleAsync(userManager, patient, "Patient");

        // 4. Seeded doctors (created by DbSeeder)
        foreach (var email in DoctorEmails)
        {
            var doctor = await userManager.FindByEmailAsync(email);
            if (doctor != null)
                await EnsurePasswordAndRoleAsync(userManager, doctor, "Doctor");
        }
    }

    private static async Task EnsurePasswordAndRoleAsync(
        UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
    {
        if (!await userManager.HasPasswordAsync(user))
            Check(await userManager.AddPasswordAsync(user, DevPassword));

        if (!await userManager.IsInRoleAsync(user, role))
            Check(await userManager.AddToRoleAsync(user, role));
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Identity seeding failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}