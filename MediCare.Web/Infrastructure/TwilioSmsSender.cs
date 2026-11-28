using System.Net.Http.Headers;
using System.Text;
using MediCare.Services.Interfaces;

namespace MediCare.Web.Infrastructure;

// Sends SMS through the Twilio REST API (no SDK needed). Settings go in user-secrets, never in git:
//   dotnet user-secrets set "Twilio:AccountSid" "AC..." --project MediCare.Web
//   dotnet user-secrets set "Twilio:AuthToken"  "..."   --project MediCare.Web
//   dotnet user-secrets set "Twilio:From"       "+1..." --project MediCare.Web   (your Twilio number)
// Without them: Development writes the SMS to the server log; other environments skip it and log an error.
// A Twilio trial account can only text numbers verified in the Twilio console. To demo without a paid plan,
// send every SMS to one verified number instead of the patient's:
//   dotnet user-secrets set "Twilio:OverrideTo" "+20..." --project MediCare.Web   (App Service: Twilio__OverrideTo)
// The text then starts with the real recipient, e.g. "[to +201001234567] ...".
public class TwilioSmsSender : ISmsSender
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(HttpClient http, IConfiguration config, IWebHostEnvironment env, ILogger<TwilioSmsSender> logger)
    {
        _http = http;
        _config = config;
        _env = env;
        _logger = logger;
    }

    public async Task SendAsync(string toPhoneNumber, string message)
    {
        var sid = _config["Twilio:AccountSid"];
        var token = _config["Twilio:AuthToken"];
        var from = _config["Twilio:From"];
        (toPhoneNumber, message) = ApplyOverride(_config["Twilio:OverrideTo"], toPhoneNumber, message);

        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(from))
        {
            if (_env.IsDevelopment())
                _logger.LogWarning("Twilio is not configured. DEV ONLY, SMS to {To}: {Message}", toPhoneNumber, message);
            else
                _logger.LogError("Twilio is not configured; an SMS to {To} was not sent.", toPhoneNumber);
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = toPhoneNumber,
                ["From"] = from,
                ["Body"] = message
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sid}:{token}")));

        try
        {
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                _logger.LogError("Twilio returned {Status} for an SMS to {To}.", (int)response.StatusCode, toPhoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sending an SMS to {To} failed.", toPhoneNumber);
        }
    }

    /// <summary>
    /// When <paramref name="overrideTo"/> is set (Twilio trial: only verified numbers), every SMS goes to that
    /// number and the text is prefixed with the intended recipient. Otherwise nothing changes.
    /// </summary>
    public static (string To, string Message) ApplyOverride(string? overrideTo, string toPhoneNumber, string message) =>
        string.IsNullOrWhiteSpace(overrideTo)
            ? (toPhoneNumber, message)
            : (overrideTo.Trim(), $"[to {toPhoneNumber}] {message}");
}
