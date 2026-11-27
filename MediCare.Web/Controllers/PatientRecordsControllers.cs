using MediCare.Services.DTOs;
using MediCare.Services.Implementations;
using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// The patient's own profile: personal and medical information + notification preferences.
[Authorize(Roles = "Patient")]
public class PatientProfileController : Controller
{
    private readonly IPatientProfileService _profiles;
    private readonly ICurrentUser _currentUser;

    public PatientProfileController(IPatientProfileService profiles, ICurrentUser currentUser)
    {
        _profiles = profiles;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profile = await _profiles.GetAsync(_currentUser.UserId!);
        return profile is null ? NotFound() : View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PatientProfileDto input)
    {
        var result = await _profiles.SaveAsync(_currentUser.UserId!, input);
        if (result.Success)
        {
            TempData["Success"] = "Profile saved.";
            return RedirectToAction(nameof(Index));
        }

        // Keep what was typed and show the error.
        TempData["Error"] = result.Error;
        var current = await _profiles.GetAsync(_currentUser.UserId!);
        input.Email = current?.Email ?? "";
        return View("Index", input);
    }
}

// Medical documents: the patient's own files; downloads are checked for every request.
[Authorize(Roles = "Patient,Doctor")]
public class DocumentsController : Controller
{
    private readonly IMedicalDocumentService _documents;
    private readonly ICurrentUser _currentUser;

    public DocumentsController(IMedicalDocumentService documents, ICurrentUser currentUser)
    {
        _documents = documents;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Categories = MedicalDocumentService.Categories;
        return View(await _documents.GetForPatientAsync(_currentUser.UserId!, _currentUser.UserId!));
    }

    [HttpPost]
    [Authorize(Roles = "Patient")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MedicalDocumentService.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Upload(string title, string category, IFormFile? file)
    {
        var userId = _currentUser.UserId!;
        var result = file is null
            ? MediCare.Services.Common.ServiceResult.Fail("Choose a file to upload.")
            : await _documents.UploadAsync(userId, userId, null, title, category, file.FileName, file.Length, file.OpenReadStream());
        if (result.Success) TempData["Success"] = "File uploaded.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    // Opens the file in the browser (PDF / image). Only the patient, or a doctor the patient booked with.
    [HttpGet]
    public async Task<IActionResult> Download(int id, bool download = false)
    {
        var doctorId = User.IsInRole("Doctor") ? await _currentUser.GetDoctorIdAsync() : null;
        var file = await _documents.OpenAsync(id, _currentUser.UserId!, doctorId);
        if (file is null)
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return download
            ? File(file.Value.Content, file.Value.Document.ContentType, file.Value.Document.OriginalFileName)
            : File(file.Value.Content, file.Value.Document.ContentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl)
    {
        var result = await _documents.DeleteAsync(id, _currentUser.UserId!);
        if (result.Success) TempData["Success"] = "File deleted.";
        else TempData["Error"] = result.Error;
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}

// Doctor: quick access to a patient's information (only patients who booked with this doctor).
[Authorize(Roles = "Doctor")]
public class DoctorPatientsController : Controller
{
    private readonly IPatientProfileService _profiles;
    private readonly IMedicalDocumentService _documents;
    private readonly ICurrentUser _currentUser;

    public DoctorPatientsController(IPatientProfileService profiles, IMedicalDocumentService documents, ICurrentUser currentUser)
    {
        _profiles = profiles;
        _documents = documents;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Details(string patientId)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null || string.IsNullOrEmpty(patientId) || !await _profiles.DoctorCanViewAsync(doctorId.Value, patientId))
            return NotFound();

        var profile = await _profiles.GetAsync(patientId);
        if (profile is null) return NotFound();

        ViewBag.History = await _profiles.GetHistoryWithDoctorAsync(doctorId.Value, patientId);
        ViewBag.Documents = await _documents.GetForPatientAsync(patientId, _currentUser.UserId!);
        ViewBag.Categories = MedicalDocumentService.Categories;
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MedicalDocumentService.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Upload(string patientId, string title, string category, IFormFile? file)
    {
        var doctorId = await _currentUser.GetDoctorIdAsync();
        if (doctorId is null) return Forbid();

        var result = file is null
            ? MediCare.Services.Common.ServiceResult.Fail("Choose a file to upload.")
            : await _documents.UploadAsync(patientId, _currentUser.UserId!, doctorId, title, category, file.FileName, file.Length, file.OpenReadStream());
        if (result.Success) TempData["Success"] = "File added to the patient's record.";
        else TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Details), new { patientId });
    }
}
