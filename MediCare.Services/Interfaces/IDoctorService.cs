using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<DoctorDto>> SearchAsync(string? specialty, string? name, string? location);
    Task<int?> GetDoctorIdByUserIdAsync(string userId);

    // One doctor (any approval state), e.g. for the booking page header or the doctor's own profile.
    Task<DoctorDto?> GetByIdAsync(int doctorId);
    // The doctor completes their profile after signing up. Changing it doesn't remove an approval.
    Task<ServiceResult> UpdateProfileAsync(int doctorId, string specialty, string clinicAddress, decimal consultationFee);

    // Admin
    Task<IEnumerable<DoctorDto>> GetAllAsync();
    Task<ServiceResult> SetApprovalAsync(int doctorId, bool approved);
}