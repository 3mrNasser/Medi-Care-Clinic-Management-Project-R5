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
    private readonly ISpecializationService _specializations;

    public DoctorProfileController(IDoctorService doctorService, ICurrentUser currentUser, ISpecializationService specializations)
    {
        _doctorService = doctorService;
        _currentUser = currentUser;
        _specializations = specializations;
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

        // Active specializations (+ the doctor's current one, even if the admin hid it since).
        var names = (await _specializations.GetActiveNamesAsync()).ToList();
        if (!string.IsNullOrWhiteSpace(doctor.Specialty) && !names.Contains(doctor.Specialty))
            names.Insert(0, doctor.Specialty);
        ViewBag.Specialties = names;
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string specialty, string clinicAddress, decimal consultationFee)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        // Only specializations from the admin's list (or keeping the current one).
        var current = await _doctorService.GetByIdAsync(doctorId.Value);
        var allowed = await _specializations.GetActiveNamesAsync();
        if (!allowed.Contains(specialty ?? "") && specialty != current?.Specialty)
        {
            TempData["Error"] = "Choose a specialty from the list.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _doctorService.UpdateProfileAsync(doctorId.Value, specialty!, clinicAddress, consultationFee);
        if (result.Success)
            TempData["Success"] = "Profile saved.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}
