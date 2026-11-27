using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

[Authorize(Roles = "Patient")]
public class MyMedicalRecordsController : Controller
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly ICurrentUser _currentUser;

    public MyMedicalRecordsController(
        IMedicalRecordService medicalRecordService,
        ICurrentUser currentUser)
    {
        _medicalRecordService = medicalRecordService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var records = await _medicalRecordService.GetForPatientAsync(_currentUser.UserId!);
        return View(records);
    }
}