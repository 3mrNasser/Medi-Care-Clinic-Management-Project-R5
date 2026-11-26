using MediCare.Data.Entities;
using MediCare.Services.Interfaces;
using MediCare.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// UI only: GET actions that render the Vitala auth screens.
// The Identity work (POST actions, ViewModels, SignInManager/UserManager) is added here by the Identity owner.
// Form field names each POST must bind are listed in docs/VITALA-FRONTEND.md.
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(IAuthService authService, SignInManager<ApplicationUser> signInManager)
    {
        _authService = authService;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult SelectRole() => View();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // The login form has no hidden returnUrl field, so keep it for the POST.
        if (!string.IsNullOrEmpty(returnUrl))
            TempData["ReturnUrl"] = returnUrl;
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
        returnUrl ??= TempData["ReturnUrl"] as string;
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (result.Role == "Patient")
            return RedirectToAction("Home", "Patient");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() =>
        Content("Access denied. You do not have permission to view this page.");

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
                return RedirectToAction(nameof(Login));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Message);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpGet]
    public IActionResult VerifyOtp() => View();

    [HttpGet]
    public IActionResult ResetPassword() => View();
}