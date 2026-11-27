using System.ComponentModel.DataAnnotations;

namespace MediCare.Web.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpViewModel
{
    [Required(ErrorMessage = "Enter the 4-digit code.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Enter the 4-digit code.")]
    public string Code { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
