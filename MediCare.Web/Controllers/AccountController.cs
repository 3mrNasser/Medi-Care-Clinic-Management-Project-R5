using System.Security.Claims;
using MediCare.Data.Entities;
using MediCare.Services.Interfaces;
using MediCare.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// Sign up, log in / out, access denied, password recovery and Google sign-in.
// Form field names each POST binds are listed in docs/VITALA-FRONTEND.md.
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountController(IAuthService authService, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
    {
        _authService = authService;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult SelectRole() => View();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Login.cshtml posts returnUrl back as a hidden field (local URLs only), so nothing is kept here.
        // (Keeping it in TempData sent a later, unrelated login to an old page.)
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.LoginAsync(model.Email, model.Password);
        if (!result.Succeeded || result.User == null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Invalid email or password.");
            return View(model);
        }

        await _signInManager.SignInAsync(result.User, isPersistent: false);

        // Only local URLs are allowed (open redirect protection).
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToRoleHome(result.Role);
    }

    // Each role's start page after signing in.
    private IActionResult RedirectToRoleHome(string? role) => role switch
    {
        "Patient" => RedirectToAction("Home", "Patient"),
        "Doctor" => RedirectToAction("Index", "DoctorAppointments"),
        "Admin" => RedirectToAction("Doctors", "Admin"),
        _ => RedirectToAction("Index", "Home")
    };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult Signup() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.SignupAsync(model.Role, model.FullName, model.Email, model.Password);
            if (result.Succeeded)
            {
                TempData["Success"] = model.Role?.Equals("Doctor", StringComparison.OrdinalIgnoreCase) == true
                    ? "Account created. Log in and complete your doctor profile; an admin will approve it."
                    : "Account created. Log in to continue.";
                return RedirectToAction(nameof(Login));
            }

            // Field errors (Email, Password) show under their field; the rest at the top.
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Key, error.Message);
        }

        return View(model);
    }

    // ---------- Password recovery: ForgotPassword → VerifyOtp → ResetPassword ----------
    // The email (and later the Identity reset token) travel between steps in TempData,
    // which is an encrypted cookie, so the browser can't read or change them.
    private const string ResetEmailKey = "ResetEmail";
    private const string ResetTokenKey = "ResetToken";

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await SendResetCodeAsync(model.Email);

        // Same answer whether the email exists or not, so nobody can find out who has an account.
        TempData[ResetEmailKey] = model.Email.Trim();
        TempData.Remove(ResetTokenKey);
        return RedirectToAction(nameof(VerifyOtp));
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        if (TempData.Peek(ResetEmailKey) is not string email)
            return RedirectToAction(nameof(ForgotPassword));
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (TempData.Peek(ResetEmailKey) is not string email)
            return RedirectToAction(nameof(ForgotPassword));
        ViewBag.Email = email;

        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.VerifyPasswordResetCodeAsync(email, model.Code);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Code), result.Error!);
            return View(model);
        }

        TempData[ResetTokenKey] = result.ResetToken;
        return RedirectToAction(nameof(ResetPassword));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp()
    {
        if (TempData.Peek(ResetEmailKey) is not string email)
            return RedirectToAction(nameof(ForgotPassword));

        await SendResetCodeAsync(email);
        TempData["Success"] = "If the email has an account, a new code is on its way.";
        return RedirectToAction(nameof(VerifyOtp));
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        if (TempData.Peek(ResetEmailKey) is null || TempData.Peek(ResetTokenKey) is null)
            return RedirectToAction(nameof(ForgotPassword));
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (TempData.Peek(ResetEmailKey) is not string email || TempData.Peek(ResetTokenKey) is not string token)
            return RedirectToAction(nameof(ForgotPassword));

        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.ResetPasswordAsync(email, token, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Key, error.Message);
            return View(model);
        }

        TempData.Remove(ResetEmailKey);
        TempData.Remove(ResetTokenKey);
        TempData["Success"] = "Your password was changed. Log in with the new password.";
        return RedirectToAction(nameof(Login));
    }

    private async Task SendResetCodeAsync(string email)
    {
        var code = await _authService.CreatePasswordResetCodeAsync(email);
        if (code == null)
            return; // no account, or a code was sent less than a minute ago

        var body = $"""
            <p>Your Vitala verification code is:</p>
            <p style="font-size:28px;font-weight:700;letter-spacing:6px">{code}</p>
            <p>It expires in 10 minutes. If you didn't ask to reset your password, ignore this email.</p>
            """;
        await _emailService.SendAsync(email.Trim(), "Your Vitala verification code", body);
    }

    // ---------- Google sign-in ----------
    // Works when Authentication:Google:ClientId and ClientSecret are set (user-secrets); otherwise the button is hidden.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        if (!schemes.Any(s => s.Name == provider))
        {
            TempData["Error"] = "Google sign-in isn't set up yet.";
            return RedirectToAction(nameof(Login));
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["Error"] = "Google sign-in was cancelled or failed.";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        var email = info?.Principal.FindFirstValue(ClaimTypes.Email);
        // Google sends email_verified; only trust verified addresses (they may match an existing account).
        var verified = info?.Principal.FindFirstValue("email_verified");
        if (info == null || string.IsNullOrEmpty(email) || string.Equals(verified, "false", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Google didn't share a verified email address.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _authService.FindOrCreateExternalUserAsync(email, info.Principal.FindFirstValue(ClaimTypes.Name));
        if (!result.Succeeded || result.User == null)
        {
            TempData["Error"] = "Could not sign in with Google.";
            return RedirectToAction(nameof(Login));
        }

        // Remember the Google login on the account (first time only).
        var logins = await _signInManager.UserManager.GetLoginsAsync(result.User);
        if (!logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
            await _signInManager.UserManager.AddLoginAsync(result.User, info);

        await _signInManager.SignInAsync(result.User, isPersistent: false);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToRoleHome(result.Role);
    }
}