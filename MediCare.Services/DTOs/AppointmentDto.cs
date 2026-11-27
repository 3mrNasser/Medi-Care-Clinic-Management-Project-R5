using MediCare.Data.Enums;

namespace MediCare.Services.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Reason { get; set; }

    /// <summary>The visit type (examination, follow-up, consultation).</summary>
    public AppointmentType Type { get; set; }

    /// <summary>The visit type's display name from <c>AppointmentFactory</c>.</summary>
    public string TypeName { get; set; } = string.Empty;
}