using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// Admin: approve doctors so patients can find and book them.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IDoctorService _doctorService;

    public AdminController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> Doctors()
    {
        var doctors = await _doctorService.GetAllAsync();
        return View(doctors);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int doctorId) => await SetApproval(doctorId, true, "Doctor approved. Patients can now find and book them.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unapprove(int doctorId) => await SetApproval(doctorId, false, "Approval removed. The doctor is hidden from patients.");

    private async Task<IActionResult> SetApproval(int doctorId, bool approved, string message)
    {
        var result = await _doctorService.SetApprovalAsync(doctorId, approved);
        if (result.Success)
            TempData["Success"] = message;
        else
            TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Doctors));
    }
}
