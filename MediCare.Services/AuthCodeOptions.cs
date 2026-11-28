namespace MediCare.Services;

/// <summary>
/// Settings for verification codes (section "Auth").
/// <para><c>Auth:FixedCode</c>: a 4-digit code (e.g. "1234") used for every verification code instead of a
/// random one, for demos without a paid SMS/email plan. Leave it empty in real use.</para>
/// </summary>
public class AuthCodeOptions
{
    public string? FixedCode { get; set; }

    /// <summary>The fixed code when it is set and exactly 4 digits; otherwise null (random codes).</summary>
    public string? ValidFixedCode =>
        FixedCode is { } c && c.Trim().Length == 4 && c.Trim().All(char.IsAsciiDigit) ? c.Trim() : null;
}
