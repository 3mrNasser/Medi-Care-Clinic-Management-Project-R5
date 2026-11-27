using MediCare.Data.Enums;

namespace MediCare.Services.DTOs;

public class SpecializationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DoctorCount { get; set; }
}

public class PatientProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodType { get; set; }
    public string? Address { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? CurrentMedications { get; set; }
    public string? MedicalHistoryNotes { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool SmsNotificationsEnabled { get; set; } = true;

    public int? Age => DateOfBirth is { } dob
        ? DateTime.Today.Year - dob.Year - (DateOnly.FromDateTime(DateTime.Today) < dob.AddYears(DateTime.Today.Year - dob.Year) ? 1 : 0)
        : null;
}

public class MedicalDocumentDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public bool UploadedByMe { get; set; }
}

public class TimeOffDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Reason { get; set; }
}

public class AdminPatientDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsLocked { get; set; }
    public int AppointmentCount { get; set; }
}

public class AdminAppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public decimal Fee { get; set; }
}

// Everything the admin analytics page (Chart.js) and the PDF / Excel export show.
public class AnalyticsDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }

    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int ApprovedDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal TotalRevenue { get; set; }

    public List<MonthStat> Months { get; set; } = new();
    public List<NameValue> StatusBreakdown { get; set; } = new();
    public List<NameValue> TopDoctors { get; set; } = new();
    public List<NameValue> TopSpecialties { get; set; } = new();
    public List<NameValue> Genders { get; set; } = new();
    public List<NameValue> AgeGroups { get; set; } = new();
}

public class MonthStat
{
    public string Label { get; set; } = string.Empty;   // "Nov 2026"
    public int Appointments { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public decimal Revenue { get; set; }
    public int NewPatients { get; set; }
}

public record NameValue(string Name, decimal Value);
