using MediCare.Data.Enums;
using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// The patient workspace (dashboard, Cart, Notifications). Patients only.
[Authorize(Roles = "Patient")]
public class PatientController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notificationService;

    public PatientController(IAppointmentService appointmentService, IDoctorService doctorService, ICurrentUser currentUser,
        INotificationService notificationService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _currentUser = currentUser;
        _notificationService = notificationService;
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
        ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(_currentUser.UserId!);
        return View();
    }

    // Cart is in the Figma sidebar but has no screen design yet: it renders an empty state so the link never 404s.
    [HttpGet]
    public IActionResult Cart() => View();

    [HttpGet]
    public IActionResult Notifications() => RedirectToAction("Index", "Notifications"); // moved: shared with doctors
}
