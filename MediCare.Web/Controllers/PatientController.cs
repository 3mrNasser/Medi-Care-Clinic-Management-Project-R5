using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// UI only: renders the Vitala patient workspace screens with placeholder content.
// Replace the static text in the views with real data (patient name, upcoming appointment) when the services exist.
public class PatientController : Controller
{
    [HttpGet]
    public IActionResult Home() => View();

    [HttpGet]
    public IActionResult AiAssistant() => View();

    // Cart and Notifications are in the Figma sidebar but have no screen design yet:
    // they render an empty state so the links never 404.
    [HttpGet]
    public IActionResult Cart() => View();

    [HttpGet]
    public IActionResult Notifications() => View();

    // Dashboard service tiles from Figma without a screen design yet: shared empty-state page.
    [HttpGet]
    public IActionResult Pharmacy() => ComingSoon("Pharmacy", "~/images/vitala/svc-pharmacy.png", "Find nearby pharmacies and medicines.");

    [HttpGet]
    public IActionResult Rewards() => ComingSoon("Reward", null, "Earn points with every visit.");

    [HttpGet]
    public IActionResult Labs() => ComingSoon("Labs", "~/images/vitala/svc-labs.png", "Book lab tests and see your results.");

    [HttpGet]
    public IActionResult Consultation() => ComingSoon("e-Consultation", "~/images/vitala/svc-econsultation.png", "Talk to a doctor online.");

    [HttpGet]
    public IActionResult MedicalHistory() => ComingSoon("Medical History", "~/images/vitala/svc-medical-history.png", "Your visits, prescriptions and records.");

    private ViewResult ComingSoon(string title, string? image, string text)
    {
        ViewData["Title"] = title;
        ViewData["Image"] = image;
        ViewData["Text"] = text;
        return View("ComingSoon");
    }
}
