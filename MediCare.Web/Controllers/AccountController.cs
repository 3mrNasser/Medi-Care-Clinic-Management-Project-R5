using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// UI only: GET actions that render the Vitala auth screens.
// The Identity work (POST actions, ViewModels, SignInManager/UserManager) is added here by the Identity owner.
// Form field names each POST must bind are listed in docs/VITALA-FRONTEND.md.
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult SelectRole() => View();

    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public IActionResult Signup() => View();

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpGet]
    public IActionResult VerifyOtp() => View();

    [HttpGet]
    public IActionResult ResetPassword() => View();
}
