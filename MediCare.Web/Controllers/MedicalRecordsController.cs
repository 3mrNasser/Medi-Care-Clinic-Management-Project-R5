using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class MedicalRecordsController : Controller
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int doctorId, int appointmentId)
    {
        var record = await _medicalRecordService.GetForDoctorAsync(doctorId, appointmentId);

        ViewBag.DoctorId = doctorId;
        ViewBag.AppointmentId = appointmentId;
        ViewBag.Diagnosis = record?.Diagnosis ?? string.Empty;
        ViewBag.Notes = record?.Notes ?? string.Empty;
        ViewBag.HasRecord = record is not null;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        int doctorId, int appointmentId, string diagnosis, string? notes)
    {
        var result = await _medicalRecordService.SaveAsync(doctorId, appointmentId, diagnosis, notes);

        if (result.Success)
            TempData["Success"] = "Medical record saved.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Edit), new { doctorId, appointmentId });
    }
}