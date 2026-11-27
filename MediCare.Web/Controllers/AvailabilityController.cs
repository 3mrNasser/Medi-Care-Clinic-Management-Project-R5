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
    private readonly ITimeOffService _timeOff;

    public AvailabilityController(
        IAvailabilityService availabilityService,
        ICurrentUser currentUser,
        ITimeOffService timeOff)
    {
        _availabilityService = availabilityService;
        _currentUser = currentUser;
        _timeOff = timeOff;
    }

    // ----- Days off (vacation): no times are offered on these dates -----
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTimeOff(DateOnly date, string? reason)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _timeOff.AddAsync(doctorId.Value, date, reason);
        if (result.Success) TempData["Success"] = $"{date:ddd d MMM} is now a day off.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTimeOff(int timeOffId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _timeOff.RemoveAsync(doctorId.Value, timeOffId);
        if (!result.Success) TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index()
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        ViewBag.DoctorId = doctorId.Value;
        ViewBag.TimeOff = await _timeOff.GetUpcomingAsync(doctorId.Value);
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