using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Appointments;
using MediCare.Services.Common;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MediCare.Services.Implementations;

/// <summary>Implements <see cref="IAdminService"/>; see the interface for what each member does.</summary>
public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notifications;

    public AdminService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<AdminPatientDto>> GetPatientsAsync(string? search)
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patient");
        var counts = (await _unitOfWork.Appointments.GetAllAsync()).GroupBy(a => a.PatientId).ToDictionary(g => g.Key, g => g.Count());
        var now = DateTimeOffset.UtcNow;

        return patients
            .Where(p => string.IsNullOrWhiteSpace(search)
                        || p.FullName.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)
                        || (p.Email ?? "").Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.FullName)
            .Select(p => new AdminPatientDto
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email ?? "",
                PhoneNumber = p.PhoneNumber,
                CreatedAt = p.CreatedAt,
                IsLocked = p.LockoutEnd is { } end && end > now,
                AppointmentCount = counts.GetValueOrDefault(p.Id)
            }).ToList();
    }

    public async Task<ServiceResult> SetPatientLockedAsync(string patientId, bool locked)
    {
        var user = await _userManager.FindByIdAsync(patientId);
        if (user is null || !await _userManager.IsInRoleAsync(user, "Patient"))
            return ServiceResult.Fail("Patient not found.");

        // Locked = can't sign in (Identity lockout). The data stays.
        await _userManager.SetLockoutEnabledAsync(user, true);
        var result = await _userManager.SetLockoutEndDateAsync(user, locked ? DateTimeOffset.MaxValue : null);
        if (!result.Succeeded) return ServiceResult.Fail("Could not update the account.");
        if (locked) await _userManager.UpdateSecurityStampAsync(user); // signs them out everywhere
        return ServiceResult.Ok();
    }

    public async Task<IReadOnlyList<AdminAppointmentDto>> GetAppointmentsAsync(AppointmentStatus? status, DateOnly? from, DateOnly? to, string? search)
    {
        var items = await _unitOfWork.Appointments.FindAsync(
            a => (status == null || a.Status == status)
                 && (from == null || a.AppointmentDate >= from)
                 && (to == null || a.AppointmentDate <= to),
            a => a.Patient,
            a => a.DoctorProfile.User);

        var q = search?.Trim();
        return items
            .Where(a => string.IsNullOrEmpty(q)
                        || a.Patient.FullName.Contains(q, StringComparison.OrdinalIgnoreCase)
                        || a.DoctorProfile.User.FullName.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
            .Select(a => new AdminAppointmentDto
            {
                Id = a.Id,
                PatientName = a.Patient.FullName,
                DoctorName = a.DoctorProfile.User.FullName,
                Specialty = a.DoctorProfile.Specialty,
                Date = a.AppointmentDate,
                StartTime = a.StartTime,
                Status = a.Status,
                Fee = AppointmentFactory.PriceFor(a.Type, a.DoctorProfile.ConsultationFee)
            }).ToList();
    }

    public async Task<ServiceResult> CancelAppointmentAsync(int appointmentId)
    {
        var a = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (a is null) return ServiceResult.Fail("Appointment not found.");
        if (a.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            return ServiceResult.Fail("Only pending or confirmed appointments can be cancelled.");

        a.Status = AppointmentStatus.Cancelled;
        _unitOfWork.Appointments.Update(a);
        await _unitOfWork.SaveChangesAsync();

        await _notifications.NotifyAsync(a.Id, NotificationType.AppointmentCancelledByClinic); // patient
        await _notifications.NotifyAsync(a.Id, NotificationType.AppointmentCancelled);         // doctor
        return ServiceResult.Ok();
    }
}

/// <summary>Implements <see cref="IReportService"/>; see the interface for what each member does.</summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<AnalyticsDto> GetAnalyticsAsync(DateOnly from, DateOnly to)
    {
        if (to < from) (from, to) = (to, from);

        var appointments = (await _unitOfWork.Appointments.FindAsync(
            a => a.AppointmentDate >= from && a.AppointmentDate <= to,
            a => a.DoctorProfile.User)).ToList();
        var doctors = (await _unitOfWork.Doctors.GetAllAsync()).ToList();
        var patients = (await _userManager.GetUsersInRoleAsync("Patient")).ToList();
        var patientIds = patients.Select(p => p.Id).ToHashSet();
        var profiles = (await _unitOfWork.PatientProfiles.GetAllAsync()).Where(p => patientIds.Contains(p.UserId)).ToList();

        static decimal Revenue(IEnumerable<Appointment> items) =>
            items.Where(a => a.Status == AppointmentStatus.Completed).Sum(a => AppointmentFactory.PriceFor(a.Type, a.DoctorProfile.ConsultationFee));

        var result = new AnalyticsDto
        {
            From = from,
            To = to,
            TotalPatients = patients.Count,
            TotalDoctors = doctors.Count,
            ApprovedDoctors = doctors.Count(d => d.IsApproved),
            TotalAppointments = appointments.Count,
            CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            TotalRevenue = Revenue(appointments)
        };

        // One entry per month in the range (max 24), even when a month is empty, so the charts have no gaps.
        var month = new DateOnly(from.Year, from.Month, 1);
        for (var i = 0; month <= to && i < 24; i++, month = month.AddMonths(1))
        {
            var m = month;
            var inMonth = appointments.Where(a => a.AppointmentDate.Year == m.Year && a.AppointmentDate.Month == m.Month).ToList();
            result.Months.Add(new MonthStat
            {
                Label = m.ToString("MMM yyyy"),
                Appointments = inMonth.Count,
                Completed = inMonth.Count(a => a.Status == AppointmentStatus.Completed),
                Cancelled = inMonth.Count(a => a.Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected),
                Revenue = Revenue(inMonth),
                NewPatients = patients.Count(p => p.CreatedAt.Year == m.Year && p.CreatedAt.Month == m.Month)
            });
        }

        result.StatusBreakdown = Enum.GetValues<AppointmentStatus>()
            .Select(s => new NameValue(s.ToString(), appointments.Count(a => a.Status == s)))
            .Where(x => x.Value > 0).ToList();

        // "Popular" = booked and not cancelled/rejected.
        var kept = appointments.Where(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed or AppointmentStatus.Completed).ToList();
        result.TopDoctors = kept.GroupBy(a => a.DoctorProfile.User.FullName)
            .Select(g => new NameValue(g.Key, g.Count())).OrderByDescending(x => x.Value).Take(5).ToList();
        result.TopSpecialties = kept.GroupBy(a => string.IsNullOrWhiteSpace(a.DoctorProfile.Specialty) ? "Not set" : a.DoctorProfile.Specialty)
            .Select(g => new NameValue(g.Key, g.Count())).OrderByDescending(x => x.Value).Take(6).ToList();

        // Demographics from patient profiles (patients without a profile count as "Not set").
        var byUser = profiles.ToDictionary(p => p.UserId);
        result.Genders = patients.GroupBy(p => byUser.GetValueOrDefault(p.Id)?.Gender ?? "Not set")
            .Select(g => new NameValue(g.Key, g.Count())).OrderByDescending(x => x.Value).ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        string AgeGroup(string userId)
        {
            if (byUser.GetValueOrDefault(userId)?.DateOfBirth is not { } dob) return "Not set";
            var age = today.Year - dob.Year - (today < dob.AddYears(today.Year - dob.Year) ? 1 : 0);
            return age switch { < 18 => "Under 18", < 35 => "18–34", < 50 => "35–49", < 65 => "50–64", _ => "65+" };
        }
        var order = new[] { "Under 18", "18–34", "35–49", "50–64", "65+", "Not set" };
        result.AgeGroups = patients.GroupBy(p => AgeGroup(p.Id))
            .Select(g => new NameValue(g.Key, g.Count())).OrderBy(x => Array.IndexOf(order, x.Name)).ToList();

        return result;
    }
}
