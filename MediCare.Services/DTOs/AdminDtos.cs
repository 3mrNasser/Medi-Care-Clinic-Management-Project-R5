namespace MediCare.Services.DTOs;

public record DoctorAdminDto(int Id, string FullName, string Email, string Specialty, bool IsApproved);

public record UserAdminDto(string Id, string FullName, string Email, string Roles);