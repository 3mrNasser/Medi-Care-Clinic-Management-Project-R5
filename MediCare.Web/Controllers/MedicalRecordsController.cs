using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Doctor")]
public class MedicalRecordsController : Controller
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly ICurrentUser _currentUser;

    public MedicalRecordsController(
        IMedicalRecordService medicalRecordService,
        ICurrentUser currentUser)
    {
        _medicalRecordService = medicalRecordService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int appointmentId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var record = await _medicalRecordService.GetForDoctorAsync(doctorId.Value, appointmentId);

        ViewBag.DoctorId = doctorId.Value;
        ViewBag.AppointmentId = appointmentId;
        ViewBag.Diagnosis = record?.Diagnosis ?? string.Empty;
        ViewBag.Notes = record?.Notes ?? string.Empty;
        ViewBag.HasRecord = record is not null;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        int appointmentId, string diagnosis, string? notes)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null)
            return Forbid();

        var result = await _medicalRecordService.SaveAsync(doctorId.Value, appointmentId, diagnosis, notes);

        if (result.Success)
            TempData["Success"] = "Medical record saved.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Edit), new { appointmentId });
    }
}