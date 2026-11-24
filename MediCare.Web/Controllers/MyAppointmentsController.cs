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
}