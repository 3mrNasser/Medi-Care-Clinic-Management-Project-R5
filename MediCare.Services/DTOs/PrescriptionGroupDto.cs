namespace MediCare.Services.DTOs;

public class PrescriptionGroupDto
{
    public int AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public List<PrescriptionDto> Items { get; set; } = new();
}