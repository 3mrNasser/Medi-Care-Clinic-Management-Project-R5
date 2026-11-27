using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// The signed-in doctor completes / edits their profile (specialty, clinic address, fee).
// A new doctor is hidden from patients until the profile is complete and an admin approves it.
[Authorize(Roles = "Doctor")]
public class DoctorProfileController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly ICurrentUser _currentUser;

    public DoctorProfileController(IDoctorService doctorService, ICurrentUser currentUser)
    {
        _doctorService = doctorService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var doctor = await _doctorService.GetByIdAsync(doctorId.Value);
        if (doctor is null)
            return Forbid();

        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string specialty, string clinicAddress, decimal consultationFee)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _doctorService.UpdateProfileAsync(doctorId.Value, specialty, clinicAddress, consultationFee);
        if (result.Success)
            TempData["Success"] = "Profile saved.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}
