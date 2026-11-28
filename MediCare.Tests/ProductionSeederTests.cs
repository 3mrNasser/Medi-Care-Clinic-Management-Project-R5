using MediCare.Data.Context;
using MediCare.Data.Entities;
using MediCare.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Tests;

public class ProductionSeederTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ServiceProvider _provider;

    public ProductionSeederTests()
    {
        _connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task Creates_roles_without_admin_when_no_settings()
    {
        using var scope = _provider.CreateScope();
        await ProductionSeeder.SeedAsync(scope.ServiceProvider, null, null);

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in IdentitySeeder.Roles)
            Assert.True(await roles.RoleExistsAsync(role));
        Assert.Empty(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().Users);
    }

    [Fact]
    public async Task Creates_admin_once_and_is_safe_to_rerun()
    {
        using (var scope = _provider.CreateScope())
            await ProductionSeeder.SeedAsync(scope.ServiceProvider, "boss@clinic.test", "secret123");
        using (var scope = _provider.CreateScope())
            await ProductionSeeder.SeedAsync(scope.ServiceProvider, "boss@clinic.test", "different456");

        using var check = _provider.CreateScope();
        var users = check.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync("boss@clinic.test");
        Assert.NotNull(admin);
        Assert.True(await users.IsInRoleAsync(admin, "Admin"));
        Assert.True(await users.CheckPasswordAsync(admin, "secret123")); // password not overwritten
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
