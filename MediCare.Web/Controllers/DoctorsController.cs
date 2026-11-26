using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class DoctorsController : Controller
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task<IActionResult> Index(string? specialty, string? name, string? location)
    {
        var doctors = await _doctorService.SearchAsync(specialty, name, location);
        return View(doctors);
    }
}