namespace MediCare.Services.Interfaces;

/// <summary>Sends email (implemented with SMTP in MediCare.Web; logs only when SMTP isn't configured).</summary>
public interface IEmailService
{
    /// <summary>Sends one HTML email.</summary>
    /// <param name="to">Recipient address.</param>
    /// <param name="subject">Subject line.</param>
    /// <param name="htmlBody">Message body as HTML.</param>
    Task SendAsync(string to, string subject, string htmlBody);
}
