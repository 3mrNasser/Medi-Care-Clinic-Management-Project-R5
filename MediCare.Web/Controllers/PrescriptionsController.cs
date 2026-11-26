using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class PrescriptionsController : Controller
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    public async Task<IActionResult> Index(int doctorId, int appointmentId)
    {
        ViewBag.DoctorId = doctorId;
        ViewBag.AppointmentId = appointmentId;

        var items = await _prescriptionService.GetForAppointmentAsync(doctorId, appointmentId);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        int doctorId, int appointmentId,
        string medicationName, string dosage, string frequency, int durationDays)
    {
        var result = await _prescriptionService.AddAsync(
            doctorId, appointmentId, medicationName, dosage, frequency, durationDays);

        if (result.Success)
            TempData["Success"] = "Medication added.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { doctorId, appointmentId });
    }
}