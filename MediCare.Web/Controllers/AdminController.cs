using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Doctors() => View(await _adminService.GetDoctorsAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        if (!await _adminService.SetDoctorApprovalAsync(id, true))
            return NotFound();

        TempData["Message"] = "Doctor approved.";
        return RedirectToAction(nameof(Doctors));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int id)
    {
        if (!await _adminService.SetDoctorApprovalAsync(id, false))
            return NotFound();

        TempData["Message"] = "Doctor approval revoked.";
        return RedirectToAction(nameof(Doctors));
    }

    [HttpGet]
    public async Task<IActionResult> Users() => View(await _adminService.GetUsersAsync());
}