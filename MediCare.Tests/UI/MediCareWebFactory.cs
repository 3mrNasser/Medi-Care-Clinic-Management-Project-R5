using System.Collections.Concurrent;
using MediCare.Data.Context;
using MediCare.Data.Entities;
using MediCare.Data.Seed;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MediCare.Tests.UI;

/// <summary>
/// Starts the real MediCare.Web app in memory for UI tests: same controllers, views, Identity, antiforgery
/// and services, but on an in-memory SQLite database, with the reminder job off and email / SMS recorded instead of sent.
/// </summary>
public sealed class MediCareWebFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    /// <summary>A day one week ahead on which Dr. Ahmed Hassan works 09:00–12:00.</summary>
    public DateOnly BookableDay { get; } = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

    /// <summary>Doctor profile id of Dr. Ahmed Hassan (seeded, approved, 400 EGP).</summary>
    public int DoctorId { get; private set; }

    /// <summary>Emails the app tried to send.</summary>
    public ConcurrentBag<(string To, string Subject)> SentEmails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Reminders:Enabled"] = "false"
        }));

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server with one shared in-memory SQLite connection.
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));

            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(new RecordingEmail(SentEmails));
            services.RemoveAll<ISmsSender>();
            services.AddSingleton<ISmsSender, SilentSms>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        IdentitySeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();

        var doctor = db.DoctorProfiles.Include(d => d.User).Single(d => d.User.Email == "ahmed.hassan@medicare.test");
        DoctorId = doctor.Id;
        db.AvailabilitySlots.Add(new AvailabilitySlot
        {
            DoctorProfileId = doctor.Id,
            DayOfWeek = BookableDay.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        });
        db.SaveChanges();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }

    private sealed class RecordingEmail(ConcurrentBag<(string, string)> sent) : IEmailService
    {
        public Task SendAsync(string to, string subject, string htmlBody)
        {
            sent.Add((to, subject));
            return Task.CompletedTask;
        }
    }

    private sealed class SilentSms : ISmsSender
    {
        public Task SendAsync(string toPhoneNumber, string message) => Task.CompletedTask;
    }
}
