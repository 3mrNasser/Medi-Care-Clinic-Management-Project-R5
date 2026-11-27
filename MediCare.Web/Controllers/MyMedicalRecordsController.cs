using MediCare.Data.Seed;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

public class MyMedicalRecordsController : Controller
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MyMedicalRecordsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    public async Task<IActionResult> Index()
    {
        var records = await _medicalRecordService.GetForPatientAsync(DbSeeder.TestPatientId);
        return View(records);
    }
}