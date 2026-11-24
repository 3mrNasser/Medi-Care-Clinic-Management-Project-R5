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

    public async Task<IActionResult> Index(string? specialty, string? name)
    {
        var doctors = await _doctorService.SearchAsync(specialty, name);
        return View(doctors);
    }
}