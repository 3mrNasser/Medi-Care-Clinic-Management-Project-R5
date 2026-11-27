using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Patient")]

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly ICurrentUser _currentUser;

    public AppointmentsController(
        IAppointmentService appointmentService,
        ICurrentUser currentUser)
    {
        _appointmentService = appointmentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> AvailableTimes(int doctorId, DateOnly date)
    {
        var times = await _appointmentService.GetAvailableTimesAsync(doctorId, date);
        return Json(times);
    }

    [HttpGet]
    public async Task<IActionResult> Book(int doctorId, DateOnly? date)
    {
        var selected = date ?? DateOnly.FromDateTime(DateTime.Now);
        ViewBag.DoctorId = doctorId;
        ViewBag.Date = selected;

        var times = await _appointmentService.GetAvailableTimesAsync(doctorId, selected);
        return View(times);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int doctorId, DateOnly date, TimeOnly time, string? reason)
    {
        var result = await _appointmentService.BookAsync(
            _currentUser.UserId!, doctorId, date, time, reason);

        if (result.Success)
            TempData["Success"] = "Appointment requested. Waiting for the doctor's confirmation.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Book), new { doctorId, date = date.ToString("yyyy-MM-dd") });
    }
}