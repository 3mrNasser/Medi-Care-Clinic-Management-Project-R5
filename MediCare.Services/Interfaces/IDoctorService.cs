using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>Doctor profiles: search for patients, the doctor's own profile, and approval by the admin.</summary>
public interface IDoctorService
{
    /// <summary>Approved doctors filtered by specialty, name and location (each filter optional).</summary>
    Task<IEnumerable<DoctorDto>> SearchAsync(string? specialty, string? name, string? location);

    /// <summary>The doctor profile id for an Identity user, or <c>null</c> if the user isn't a doctor.</summary>
    Task<int?> GetDoctorIdByUserIdAsync(string userId);

    /// <summary>One doctor (any approval state), e.g. for the booking page header or the doctor's own profile.</summary>
    Task<DoctorDto?> GetByIdAsync(int doctorId);

    /// <summary>The doctor completes or edits their profile. Changing it doesn't remove an approval.</summary>
    Task<ServiceResult> UpdateProfileAsync(int doctorId, string specialty, string clinicAddress, decimal consultationFee);

    /// <summary>Admin: every doctor, approved or not.</summary>
    Task<IEnumerable<DoctorDto>> GetAllAsync();

    /// <summary>Admin: approves a doctor (they appear in search and can be booked) or removes the approval.</summary>
    Task<ServiceResult> SetApprovalAsync(int doctorId, bool approved);
}
