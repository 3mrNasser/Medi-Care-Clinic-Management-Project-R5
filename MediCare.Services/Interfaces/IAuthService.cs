using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>Accounts on top of ASP.NET Core Identity: sign-up, login, password recovery and Google sign-in.</summary>
public interface IAuthService
{
    /// <summary>Creates an account with the role <c>Patient</c> or <c>Doctor</c> (doctors also get an unapproved profile).</summary>
    /// <param name="role">"Patient" or "Doctor"; anything else becomes Patient.</param>
    /// <param name="fullName">Display name.</param>
    /// <param name="email">Email, also used as the user name.</param>
    /// <param name="password">Password; checked against the Identity password rules.</param>
    Task<AuthResult> SignupAsync(string? role, string fullName, string email, string password);

    /// <summary>Checks the email and password and returns the user's role, or why login failed (wrong details, locked).</summary>
    Task<LoginResult> LoginAsync(string email, string password);

    /// <summary>
    /// Starts password recovery: creates a 4-digit code to email.
    /// Returns <c>null</c> when no account uses this email; the caller must not reveal that.
    /// </summary>
    Task<string?> CreatePasswordResetCodeAsync(string email);

    /// <summary>Checks the 4-digit code. On success returns an Identity reset token for <see cref="ResetPasswordAsync"/>.</summary>
    Task<ResetCodeResult> VerifyPasswordResetCodeAsync(string email, string code);

    /// <summary>Sets a new password using the token from <see cref="VerifyPasswordResetCodeAsync"/>.</summary>
    Task<AuthResult> ResetPasswordAsync(string email, string resetToken, string newPassword);

    /// <summary>Google / external sign-in: finds the account by email, or creates a Patient account.</summary>
    Task<LoginResult> FindOrCreateExternalUserAsync(string email, string? fullName);
}
