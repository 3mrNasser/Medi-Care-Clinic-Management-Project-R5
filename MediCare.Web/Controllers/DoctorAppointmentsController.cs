using MediCare.Data.Enums;
using MediCare.Services.Common;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class DoctorAppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;

    public DoctorAppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<IActionResult> Index(int doctorId)
    {
        ViewBag.DoctorId = doctorId;
        var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Events(int doctorId)
    {
        var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
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
    public async Task<IActionResult> Confirm(int doctorId, int appointmentId)
    {
        var result = await _appointmentService.ConfirmAsync(doctorId, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { doctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int doctorId, int appointmentId)
    {
        var result = await _appointmentService.RejectAsync(doctorId, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { doctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int doctorId, int appointmentId)
    {
        var result = await _appointmentService.CompleteAsync(doctorId, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { doctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NoShow(int doctorId, int appointmentId)
    {
        var result = await _appointmentService.MarkNoShowAsync(doctorId, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { doctorId });
    }
}