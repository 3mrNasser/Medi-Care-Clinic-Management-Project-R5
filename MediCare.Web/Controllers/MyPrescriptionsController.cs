using MediCare.Data.Seed;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class MyPrescriptionsController : Controller
{
    private readonly IPrescriptionService _prescriptionService;

    public MyPrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    public async Task<IActionResult> Index()
    {
        var groups = await _prescriptionService.GetForPatientAsync(DbSeeder.TestPatientId);
        return View(groups);
    }
}