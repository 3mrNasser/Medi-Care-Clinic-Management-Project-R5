namespace MediCare.Services.DTOs;

public class DoctorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public bool IsApproved { get; set; }
    public string Email { get; set; } = string.Empty;
}