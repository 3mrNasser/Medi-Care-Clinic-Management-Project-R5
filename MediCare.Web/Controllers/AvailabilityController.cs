using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class AvailabilityController : Controller
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    public async Task<IActionResult> Index(int doctorId)
    {
        ViewBag.DoctorId = doctorId;
        var slots = await _availabilityService.GetByDoctorAsync(doctorId);
        return View(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int doctorId, DayOfWeek day, TimeOnly start, TimeOnly end)
    {
        var result = await _availabilityService.AddAsync(doctorId, day, start, end);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { doctorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int doctorId, int slotId)
    {
        var result = await _availabilityService.RemoveAsync(doctorId, slotId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { doctorId });
    }
}