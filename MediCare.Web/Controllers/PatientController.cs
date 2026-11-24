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
}
