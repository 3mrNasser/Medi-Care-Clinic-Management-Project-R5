using MediCare.Data.Enums;
using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Patient")]

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly ICurrentUser _currentUser;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        ICurrentUser currentUser)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
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
        // Only approved doctors can be booked; show who is being booked (name, specialty, clinic, fee).
        var doctor = await _doctorService.GetByIdAsync(doctorId);
        if (doctor is null || !doctor.IsApproved)
        {
            TempData["Error"] = "This doctor isn't available for booking.";
            return RedirectToAction("Index", "Doctors");
        }

        var selected = date ?? DateOnly.FromDateTime(DateTime.Now);
        ViewBag.Doctor = doctor;
        ViewBag.DoctorId = doctorId;
        ViewBag.Date = selected;

        var times = await _appointmentService.GetAvailableTimesAsync(doctorId, selected);
        return View(times);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int doctorId, DateOnly date, TimeOnly time, string? reason,
        AppointmentType type = AppointmentType.FirstVisit)
    {
        var result = await _appointmentService.BookAsync(
            _currentUser.UserId!, doctorId, date, time, reason, type);

        if (result.Success)
            TempData["Success"] = "Appointment requested. Waiting for the doctor's confirmation.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Book), new { doctorId, date = date.ToString("yyyy-MM-dd") });
    }
}