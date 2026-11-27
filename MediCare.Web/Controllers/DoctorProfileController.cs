using MediCare.Services.Common;
using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Doctor")]
public class DoctorProfileController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly ICurrentUser _currentUser;

    public DoctorProfileController(
        IDoctorService doctorService,
        ICurrentUser currentUser)
    {
        _doctorService = doctorService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var profile = await _doctorService.GetProfileAsync(doctorId.Value);
        if (profile is null)
            return NotFound();

        ViewBag.Specialties = Specialties.All;
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string specialty, string clinicAddress, string consultationFee)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        if (!decimal.TryParse(consultationFee, NumberStyles.Number, CultureInfo.InvariantCulture, out var fee))
        {
            TempData["Error"] = "Consultation fee must be a number.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _doctorService.UpdateProfileAsync(doctorId.Value, specialty, clinicAddress, fee);

        if (result.Success)
            TempData["Success"] = "Profile saved.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}