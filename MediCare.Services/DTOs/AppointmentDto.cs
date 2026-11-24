using MediCare.Data.Enums;

namespace MediCare.Services.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Reason { get; set; }
}