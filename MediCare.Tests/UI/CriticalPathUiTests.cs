using System.Net;
using System.Text.RegularExpressions;
using MediCare.Data.Context;
using MediCare.Data.Enums;
using MediCare.Data.Seed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Tests.UI;

/// <summary>
/// UI tests for the critical paths: they request the real pages and post the real forms
/// (with antiforgery tokens and login cookies), like a browser does.
/// </summary>
public sealed partial class CriticalPathUiTests : IClassFixture<MediCareWebFactory>
{
    private const string PatientEmail = "patient.test@medicare.test";
    private const string DoctorEmail = "ahmed.hassan@medicare.test";

    private readonly MediCareWebFactory _factory;

    public CriticalPathUiTests(MediCareWebFactory factory) => _factory = factory;

    // Each call is a new "browser" with its own cookies. Redirects are not followed so they can be checked.
    private HttpClient NewBrowser() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    [GeneratedRegex("name=\"time\" value=\"(\\d{2}:\\d{2})\"")]
    private static partial Regex TimeRegex();

    private static async Task<string> TokenFromAsync(HttpClient browser, string url)
    {
        var html = await browser.GetStringAsync(url);
        var match = TokenRegex().Match(html);
        Assert.True(match.Success, $"No antiforgery token on {url}");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static async Task<HttpResponseMessage> PostFormAsync(HttpClient browser, string url, string token,
        params (string Key, string Value)[] fields)
    {
        var form = fields.Select(f => new KeyValuePair<string, string>(f.Key, f.Value))
            .Append(new("__RequestVerificationToken", token));
        return await browser.PostAsync(url, new FormUrlEncodedContent(form));
    }

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient browser, string email, string password = IdentitySeeder.DevPassword)
    {
        var token = await TokenFromAsync(browser, "/Account/Login");
        return await PostFormAsync(browser, "/Account/Login", token, ("Email", email), ("Password", password));
    }

    private static string Location(HttpResponseMessage response) =>
        response.Headers.Location?.OriginalString ?? string.Empty;

    /// <summary>Books the first free time with the seeded doctor through the booking form; returns the time.</summary>
    private async Task<string> BookThroughFormAsync(HttpClient patient, AppointmentType type = AppointmentType.FirstVisit)
    {
        var url = $"/Appointments/Book?doctorId={_factory.DoctorId}&date={_factory.BookableDay:yyyy-MM-dd}";
        var html = await patient.GetStringAsync(url);
        var time = TimeRegex().Match(html);
        Assert.True(time.Success, "The booking page shows no free times.");
        var token = WebUtility.HtmlDecode(TokenRegex().Match(html).Groups[1].Value);

        var response = await PostFormAsync(patient, "/Appointments/Book", token,
            ("doctorId", _factory.DoctorId.ToString()),
            ("date", _factory.BookableDay.ToString("yyyy-MM-dd")),
            ("time", time.Groups[1].Value),
            ("type", type.ToString()),
            ("reason", "UI test visit"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return time.Groups[1].Value;
    }

    private T WithDb<T>(Func<ApplicationDbContext, T> query)
    {
        using var scope = _factory.Services.CreateScope();
        return query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    [Fact]
    public async Task Home_page_loads_for_visitors()
    {
        var response = await NewBrowser().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Protected_page_sends_visitors_to_login()
    {
        var response = await NewBrowser().GetAsync("/MyAppointments");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", Location(response));
    }

    [Fact]
    public async Task Wrong_password_stays_on_login_with_an_error()
    {
        var response = await LoginAsync(NewBrowser(), PatientEmail, "not-the-password");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("validation-summary-errors", html);
        Assert.DoesNotContain(response.Headers, h => h.Key == "Set-Cookie" && h.Value.Any(v => v.StartsWith(".AspNetCore.Identity")));
    }

    [Theory]
    [InlineData(PatientEmail, "/Patient/Home")]
    [InlineData(DoctorEmail, "/DoctorAppointments")]
    [InlineData(IdentitySeeder.AdminEmail, "/Admin/Dashboard")]
    public async Task Login_sends_each_role_to_its_home(string email, string home)
    {
        var browser = NewBrowser();

        var response = await LoginAsync(browser, email);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(home, Location(response));
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync(home)).StatusCode);
    }

    [Fact]
    public async Task Patient_cannot_open_admin_pages()
    {
        var patient = NewBrowser();
        await LoginAsync(patient, PatientEmail);

        var response = await patient.GetAsync("/Admin/Dashboard");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", Location(response));
    }

    [Fact]
    public async Task Patient_books_through_the_form_and_sees_it_pending()
    {
        var patient = NewBrowser();
        await LoginAsync(patient, PatientEmail);

        var time = await BookThroughFormAsync(patient, AppointmentType.Consultation);

        var page = await patient.GetStringAsync("/MyAppointments");
        Assert.Contains("Dr. Ahmed Hassan", page);
        Assert.Contains(time, page);
        Assert.Contains("Pending", page);
        Assert.Contains("Consultation", page);
        Assert.True(WithDb(db => db.Appointments.Any(a =>
            a.DoctorProfileId == _factory.DoctorId && a.Status == AppointmentStatus.Pending && a.Type == AppointmentType.Consultation)));
    }

    [Fact]
    public async Task Booking_form_without_antiforgery_token_is_rejected()
    {
        var patient = NewBrowser();
        await LoginAsync(patient, PatientEmail);

        var response = await patient.PostAsync("/Appointments/Book", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["doctorId"] = _factory.DoctorId.ToString(),
            ["date"] = _factory.BookableDay.ToString("yyyy-MM-dd"),
            ["time"] = "11:30"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Doctor_confirms_and_the_patient_is_notified()
    {
        var patient = NewBrowser();
        await LoginAsync(patient, PatientEmail);
        var time = TimeOnly.Parse(await BookThroughFormAsync(patient));
        var appointmentId = WithDb(db => db.Appointments.AsNoTracking().Single(a =>
            a.DoctorProfileId == _factory.DoctorId && a.AppointmentDate == _factory.BookableDay && a.StartTime == time).Id);

        var doctor = NewBrowser();
        await LoginAsync(doctor, DoctorEmail);
        var token = await TokenFromAsync(doctor, "/DoctorAppointments");
        var confirm = await PostFormAsync(doctor, "/DoctorAppointments/Confirm", token, ("appointmentId", appointmentId.ToString()));

        Assert.Equal(HttpStatusCode.Redirect, confirm.StatusCode);
        Assert.Equal(AppointmentStatus.Confirmed, WithDb(db => db.Appointments.AsNoTracking().Single(a => a.Id == appointmentId).Status));
        var notifications = await patient.GetStringAsync("/Notifications");
        Assert.Contains("Appointment confirmed", notifications);
        Assert.Contains(_factory.SentEmails, e => e.To == PatientEmail);
    }

    [Fact]
    public async Task Admin_dashboard_and_pdf_report_work()
    {
        var admin = NewBrowser();
        await LoginAsync(admin, IdentitySeeder.AdminEmail);

        var dashboard = await admin.GetStringAsync("/Admin/Dashboard");
        Assert.Contains("Revenue (EGP)", dashboard);

        var pdf = await admin.GetAsync("/Admin/ExportPdf");
        Assert.Equal(HttpStatusCode.OK, pdf.StatusCode);
        Assert.Equal("application/pdf", pdf.Content.Headers.ContentType?.MediaType);
        var bytes = await pdf.Content.ReadAsByteArrayAsync();
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }
}
