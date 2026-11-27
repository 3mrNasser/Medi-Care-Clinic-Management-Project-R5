using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<DoctorDto>> SearchAsync(string? specialty, string? name, string? location);
    Task<int?> GetDoctorIdByUserIdAsync(string userId);
    Task<DoctorProfileDto?> GetProfileAsync(int doctorId);
    Task<ServiceResult> UpdateProfileAsync(int doctorId, string specialty, string clinicAddress, decimal consultationFee);
}