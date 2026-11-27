using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Doctor")]
public class PrescriptionsController : Controller
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly ICurrentUser _currentUser;

    public PrescriptionsController(
        IPrescriptionService prescriptionService,
        ICurrentUser currentUser)
    {
        _prescriptionService = prescriptionService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(int appointmentId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        ViewBag.DoctorId = doctorId.Value;
        ViewBag.AppointmentId = appointmentId;

        var items = await _prescriptionService.GetForAppointmentAsync(doctorId.Value, appointmentId);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        int appointmentId,
        string medicationName, string dosage, string frequency, int durationDays)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _prescriptionService.AddAsync(
            doctorId.Value, appointmentId, medicationName, dosage, frequency, durationDays);

        if (result.Success)
            TempData["Success"] = "Medication added.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { appointmentId });
    }
}