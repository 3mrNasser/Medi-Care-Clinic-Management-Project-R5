using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MediCare.Tests.UI;

/// <summary>Password recovery with Auth:ShowCodeOnScreen (code shown on the Verify page instead of only emailed).</summary>
public sealed partial class ScreenCodeUiTests : IClassFixture<MediCareWebFactory>
{
    private readonly MediCareWebFactory _factory;

    public ScreenCodeUiTests(MediCareWebFactory factory) => _factory = factory;

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    [GeneratedRegex("data-vt-screen-code>(\\d{4})<")]
    private static partial Regex ScreenCodeRegex();

    private static async Task<HttpResponseMessage> PostAsync(HttpClient browser, string url, params (string, string)[] fields)
    {
        var html = await browser.GetStringAsync(url);
        var token = WebUtility.HtmlDecode(TokenRegex().Match(html).Groups[1].Value);
        var form = fields.Select(f => new KeyValuePair<string, string>(f.Item1, f.Item2))
            .Append(new("__RequestVerificationToken", token));
        return await browser.PostAsync(url, new FormUrlEncodedContent(form));
    }

    [Fact]
    public async Task Code_is_shown_on_screen_and_accepted_when_setting_is_on()
    {
        var app = _factory.WithWebHostBuilder(b => b.ConfigureAppConfiguration(c =>
            c.AddInMemoryCollection(new Dictionary<string, string?> { ["Auth:ShowCodeOnScreen"] = "true" })));
        var browser = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var sent = await PostAsync(browser, "/Account/ForgotPassword", ("Email", "patient.test@medicare.test"));
        Assert.Equal(HttpStatusCode.Redirect, sent.StatusCode);

        var page = await browser.GetStringAsync("/Account/VerifyOtp");
        var code = ScreenCodeRegex().Match(page);
        Assert.True(code.Success, "The code is not shown on the Verify page.");

        var verified = await PostAsync(browser, "/Account/VerifyOtp", ("Code", code.Groups[1].Value));
        Assert.Equal(HttpStatusCode.Redirect, verified.StatusCode);
        Assert.EndsWith("/Account/ResetPassword", verified.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Code_is_not_shown_by_default()
    {
        var browser = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        await PostAsync(browser, "/Account/ForgotPassword", ("Email", "ahmed.hassan@medicare.test"));
        var page = await browser.GetStringAsync("/Account/VerifyOtp");

        Assert.DoesNotMatch(ScreenCodeRegex(), page);
    }
}
