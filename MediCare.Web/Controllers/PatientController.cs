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
}
