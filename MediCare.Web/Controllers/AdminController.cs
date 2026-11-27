using MediCare.Data.Enums;
using MediCare.Services.Interfaces;
using MediCare.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// Admin: analytics dashboard (+ PDF/Excel export), doctors (approval), patients (lock / unlock),
// all appointments (filter / cancel) and specializations.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IAdminService _adminService;
    private readonly IReportService _reportService;
    private readonly ISpecializationService _specializations;

    public AdminController(IDoctorService doctorService, IAdminService adminService, IReportService reportService,
        ISpecializationService specializations)
    {
        _doctorService = doctorService;
        _adminService = adminService;
        _reportService = reportService;
        _specializations = specializations;
    }

    // ---------- Dashboard (Chart.js) ----------
    [HttpGet]
    public async Task<IActionResult> Dashboard(DateOnly? from, DateOnly? to)
    {
        var (f, t) = Range(from, to);
        return View(await _reportService.GetAnalyticsAsync(f, t));
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(DateOnly? from, DateOnly? to)
    {
        var (f, t) = Range(from, to);
        var bytes = ReportExporter.ToExcel(await _reportService.GetAnalyticsAsync(f, t));
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"medicare-report-{f:yyyyMMdd}-{t:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(DateOnly? from, DateOnly? to)
    {
        var (f, t) = Range(from, to);
        var bytes = ReportExporter.ToPdf(await _reportService.GetAnalyticsAsync(f, t));
        return File(bytes, "application/pdf", $"medicare-report-{f:yyyyMMdd}-{t:yyyyMMdd}.pdf");
    }

    // Default: the last 12 months (including this one).
    private static (DateOnly From, DateOnly To) Range(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var t = to ?? new DateOnly(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
        var f = from ?? new DateOnly(t.Year, t.Month, 1).AddMonths(-11);
        if (f > t) (f, t) = (t, f);
        if (t.DayNumber - f.DayNumber > 731) f = t.AddYears(-2); // keep reports to 2 years
        return (f, t);
    }

    // ---------- Doctors ----------
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
        Flash(result.Success, message, result.Error);
        return RedirectToAction(nameof(Doctors));
    }

    // ---------- Patients ----------
    [HttpGet]
    public async Task<IActionResult> Patients(string? q)
    {
        ViewBag.Query = q;
        return View(await _adminService.GetPatientsAsync(q));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPatientLocked(string patientId, bool locked, string? q)
    {
        var result = await _adminService.SetPatientLockedAsync(patientId, locked);
        Flash(result.Success, locked ? "Account blocked. The patient can't sign in." : "Account unblocked.", result.Error);
        return RedirectToAction(nameof(Patients), new { q });
    }

    // ---------- Appointments ----------
    [HttpGet]
    public async Task<IActionResult> Appointments(AppointmentStatus? status, DateOnly? from, DateOnly? to, string? q)
    {
        ViewBag.Status = status;
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.Query = q;
        return View(await _adminService.GetAppointmentsAsync(status, from, to, q));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int appointmentId, string? returnUrl)
    {
        var result = await _adminService.CancelAppointmentAsync(appointmentId);
        Flash(result.Success, "Appointment cancelled. The patient and the doctor were notified.", result.Error);
        // Back to the same filtered list (local URLs only).
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Appointments));
    }

    // ---------- Specializations ----------
    [HttpGet]
    public async Task<IActionResult> Specializations() => View(await _specializations.GetAllAsync(includeHidden: true));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSpecialization(string name, string? description)
    {
        var result = await _specializations.AddAsync(name, description);
        Flash(result.Success, "Specialization added.", result.Error);
        return RedirectToAction(nameof(Specializations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSpecialization(int id, string name, string? description, bool isActive)
    {
        var result = await _specializations.UpdateAsync(id, name, description, isActive);
        Flash(result.Success, "Specialization saved.", result.Error);
        return RedirectToAction(nameof(Specializations));
    }

    private void Flash(bool success, string message, string? error)
    {
        if (success) TempData["Success"] = message;
        else TempData["Error"] = error;
    }
}
