using MediCare.Services.Interfaces;
using MediCare.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// UI only: GET actions that render the Vitala auth screens.
// The Identity work (POST actions, ViewModels, SignInManager/UserManager) is added here by the Identity owner.
// Form field names each POST must bind are listed in docs/VITALA-FRONTEND.md.
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult SelectRole() => View();

    [HttpGet]
    public IActionResult Login() => View();

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

        // The Signup view reads the role from ?role= in the URL. Keep it, so a doctor who
        // makes a typo is not silently switched to Patient when the form is shown again.
        Request.QueryString = new QueryString("?role=" + Uri.EscapeDataString(model.Role ?? ""));
        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpGet]
    public IActionResult VerifyOtp() => View();

    [HttpGet]
    public IActionResult ResetPassword() => View();
}