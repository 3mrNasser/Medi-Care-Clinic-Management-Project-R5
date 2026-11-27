namespace MediCare.Data.Entities;

// A medical specialization managed by the admin (e.g. "Cardiology"). Doctors pick one; patients filter by it.
// DoctorProfile.Specialty keeps the name (text), so existing data and queries keep working.
public class Specialization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

// Personal and medical information of a patient (one per patient account).
public class PatientProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }          // "Male" | "Female"
    public string? BloodType { get; set; }       // "A+", "O-", ...
    public string? Address { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? CurrentMedications { get; set; }
    public string? MedicalHistoryNotes { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

// A file (test result, X-ray, report) attached to a patient. Stored on disk outside wwwroot; downloaded through an authorized action.
public class MedicalDocument
{
    public int Id { get; set; }

    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser Patient { get; set; } = null!;

    public string UploadedByUserId { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Other";   // "Test result" | "X-ray" | "Report" | "Other"
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;   // random name on disk, never the user's name
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}

// A day the doctor doesn't work (vacation, conference...). No times are offered on that date.
public class DoctorTimeOff
{
    public int Id { get; set; }

    public int DoctorProfileId { get; set; }
    public DoctorProfile DoctorProfile { get; set; } = null!;

    public DateOnly Date { get; set; }
    public string? Reason { get; set; }
}
