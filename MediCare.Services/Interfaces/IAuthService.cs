using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResult> SignupAsync(string? role, string fullName, string email, string password);
    Task<LoginResult> LoginAsync(string email, string password);

    // Password recovery (Forgot password → 4-digit code → new password).
    // Returns the code to email, or null when no account uses this email (the caller must not reveal that).
    Task<string?> CreatePasswordResetCodeAsync(string email);
    // Checks the code. On success returns an Identity password-reset token for ResetPasswordAsync.
    Task<ResetCodeResult> VerifyPasswordResetCodeAsync(string email, string code);
    Task<AuthResult> ResetPasswordAsync(string email, string resetToken, string newPassword);

    // Google / external sign-in: finds the account by email, or creates a Patient account.
    Task<LoginResult> FindOrCreateExternalUserAsync(string email, string? fullName);
}