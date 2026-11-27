using MediCare.Data.Enums;
using MediCare.Services.Common;
using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Doctor")]
public class DoctorAppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly ICurrentUser _currentUser;

    public DoctorAppointmentsController(
        IAppointmentService appointmentService,
        ICurrentUser currentUser)
    {
        _appointmentService = appointmentService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        ViewBag.DoctorId = doctorId.Value;
        var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId.Value);
        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Events()
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId.Value);
        var length = TimeSpan.FromMinutes(BookingRules.AppointmentMinutes);

        var events = appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.Rejected)
            .Select(a =>
            {
                var start = a.Date.ToDateTime(a.StartTime);
                return new
                {
                    id = a.Id,
                    title = a.PatientName,
                    start = start.ToString("s"),
                    end = (start + length).ToString("s"),
                    status = a.Status.ToString()
                };
            });

        return Json(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int appointmentId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _appointmentService.ConfirmAsync(doctorId.Value, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int appointmentId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _appointmentService.RejectAsync(doctorId.Value, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int appointmentId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _appointmentService.CompleteAsync(doctorId.Value, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NoShow(int appointmentId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _appointmentService.MarkNoShowAsync(doctorId.Value, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}