namespace MediCare.Data.Entities;

public class DoctorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Specialty { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public bool IsApproved { get; set; }
}