using MediCare.Data.Enums;
using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// The patient workspace (dashboard, AI assistant, and pages still to be designed). Patients only.
[Authorize(Roles = "Patient")]
public class PatientController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly ICurrentUser _currentUser;

    public PatientController(IAppointmentService appointmentService, IDoctorService doctorService, ICurrentUser currentUser)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _currentUser = currentUser;
    }

    // Dashboard: the patient's name (FullName claim) and their next Pending/Confirmed appointment.
    [HttpGet]
    public async Task<IActionResult> Home()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var time = TimeOnly.FromDateTime(now);

        var appointments = await _appointmentService.GetPatientAppointmentsAsync(_currentUser.UserId!);
        var next = appointments
            .Where(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed)
            .Where(a => a.Date > today || (a.Date == today && a.StartTime >= time))
            .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
            .FirstOrDefault();

        ViewBag.NextAppointment = next;
        ViewBag.NextDoctor = next is null ? null : await _doctorService.GetByIdAsync(next.DoctorId);
        return View();
    }

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
