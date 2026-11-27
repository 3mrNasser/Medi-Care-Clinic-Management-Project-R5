using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class DoctorsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;
    private readonly ISpecializationService _specializations;

    public DoctorsController(IDoctorService doctorService, IAppointmentService appointmentService, ISpecializationService specializations)
    {
        _doctorService = doctorService;
        _appointmentService = appointmentService;
        _specializations = specializations;
    }

    // Search by specialty, name, location and (optional) a date: only doctors with a free time that day.
    public async Task<IActionResult> Index(string? specialty, string? name, string? location, DateOnly? date)
    {
        var doctors = (await _doctorService.SearchAsync(specialty, name, location)).ToList();

        if (date is { } day)
        {
            var free = new List<MediCare.Services.DTOs.DoctorDto>();
            foreach (var d in doctors)
                if ((await _appointmentService.GetAvailableTimesAsync(d.Id, day)).Any())
                    free.Add(d);
            doctors = free;
        }

        ViewBag.Date = date;
        ViewBag.Specialties = await _specializations.GetActiveNamesAsync();
        return View(doctors);
    }
}
