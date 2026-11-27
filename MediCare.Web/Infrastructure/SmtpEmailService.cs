using System.Net;
using System.Net.Mail;
using MediCare.Services.Interfaces;

namespace MediCare.Web.Infrastructure;

// Sends email through SMTP. Settings (put the password in user-secrets, never in appsettings.json):
//   "Email": { "Host": "smtp.gmail.com", "Port": 587, "UserName": "...", "Password": "...", "From": "...", "EnableSsl": true }
// With no Host configured:
//   - Development: the email (including any code) is written to the server log, so the flow can be tested.
//   - Other environments: nothing is sent and an error is logged (without the content).
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, IWebHostEnvironment env, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var section = _config.GetSection("Email");
        var host = section["Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            if (_env.IsDevelopment())
                _logger.LogWarning("Email is not configured (Email:Host). DEV ONLY, email to {To}: {Subject}\n{Body}", to, subject, htmlBody);
            else
                _logger.LogError("Email is not configured (Email:Host); an email to {To} was not sent.", to);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(section["From"] ?? section["UserName"] ?? "no-reply@vitala.local", "Vitala"),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        using var client = new SmtpClient(host, int.TryParse(section["Port"], out var port) ? port : 587)
        {
            EnableSsl = !bool.TryParse(section["EnableSsl"], out var ssl) || ssl,
            Credentials = string.IsNullOrEmpty(section["UserName"])
                ? null
                : new NetworkCredential(section["UserName"], section["Password"])
        };

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Never break the page because email failed; the user can press "Resend code".
            _logger.LogError(ex, "Sending email to {To} failed.", to);
        }
    }
}
