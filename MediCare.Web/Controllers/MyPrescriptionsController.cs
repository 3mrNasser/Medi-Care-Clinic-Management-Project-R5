using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Patient")]
public class MyPrescriptionsController : Controller
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly ICurrentUser _currentUser;

    public MyPrescriptionsController(
        IPrescriptionService prescriptionService,
        ICurrentUser currentUser)
    {
        _prescriptionService = prescriptionService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var groups = await _prescriptionService.GetForPatientAsync(_currentUser.UserId!);
        return View(groups);
    }
}