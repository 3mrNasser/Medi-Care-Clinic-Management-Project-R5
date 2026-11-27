using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IAdminService
{
    Task<IReadOnlyList<DoctorAdminDto>> GetDoctorsAsync();
    Task<bool> SetDoctorApprovalAsync(int doctorProfileId, bool isApproved);
    Task<IReadOnlyList<UserAdminDto>> GetUsersAsync();
}