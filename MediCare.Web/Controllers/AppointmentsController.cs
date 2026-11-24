using MediCare.Data.Seed;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
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
            DbSeeder.TestPatientId, doctorId, date, time, reason);

        if (result.Success)
            TempData["Success"] = "Appointment requested. Waiting for the doctor's confirmation.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Book), new { doctorId, date = date.ToString("yyyy-MM-dd") });
    }
}