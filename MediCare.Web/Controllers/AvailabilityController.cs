using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Doctor")]
public class AvailabilityController : Controller
{
    private readonly IAvailabilityService _availabilityService;
    private readonly ICurrentUser _currentUser;

    public AvailabilityController(
        IAvailabilityService availabilityService,
        ICurrentUser currentUser)
    {
        _availabilityService = availabilityService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        ViewBag.DoctorId = doctorId.Value;
        var slots = await _availabilityService.GetByDoctorAsync(doctorId.Value);
        return View(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(DayOfWeek day, TimeOnly start, TimeOnly end)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _availabilityService.AddAsync(doctorId.Value, day, start, end);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int slotId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _availabilityService.RemoveAsync(doctorId.Value, slotId);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}