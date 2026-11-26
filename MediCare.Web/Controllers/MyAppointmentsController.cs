using MediCare.Data.Seed;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class MyAppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;

    public MyAppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public async Task<IActionResult> Index()
    {
        var appointments = await _appointmentService.GetPatientAppointmentsAsync(DbSeeder.TestPatientId);
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int appointmentId)
    {
        var result = await _appointmentService.CancelAsync(DbSeeder.TestPatientId, appointmentId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Reschedule(int appointmentId, DateOnly? date)
    {
        var mine = await _appointmentService.GetPatientAppointmentsAsync(DbSeeder.TestPatientId);
        var appointment = mine.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment is null)
            return NotFound();

        var selected = date ?? appointment.Date;
        var times = await _appointmentService.GetAvailableTimesAsync(appointment.DoctorId, selected);

        ViewBag.AppointmentId = appointmentId;
        ViewBag.Date = selected;
        ViewBag.Current = $"{appointment.Date:yyyy-MM-dd} {appointment.StartTime:HH:mm}";
        return View(times);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(int appointmentId, DateOnly date, TimeOnly time)
    {
        var result = await _appointmentService.RescheduleAsync(
            DbSeeder.TestPatientId, appointmentId, date, time);

        if (result.Success)
        {
            TempData["Success"] = "Appointment rescheduled. Waiting for the doctor's confirmation.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Reschedule),
            new { appointmentId, date = date.ToString("yyyy-MM-dd") });
    }
}