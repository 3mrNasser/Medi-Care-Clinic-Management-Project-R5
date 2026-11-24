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
}