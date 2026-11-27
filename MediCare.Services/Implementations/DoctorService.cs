using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Common;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Implementations;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;

    public DoctorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DoctorDto>> SearchAsync(string? specialty, string? name, string? location)
    {
        var doctors = await _unitOfWork.Doctors.FindAsync(
            d => d.IsApproved
                 && (string.IsNullOrEmpty(specialty) || d.Specialty == specialty)
                 && (string.IsNullOrEmpty(name) || d.User.FullName.Contains(name))
                 && (string.IsNullOrEmpty(location) || d.ClinicAddress.Contains(location)),
            d => d.User);

        return doctors.Select(d => new DoctorDto
        {
            Id = d.Id,
            FullName = d.User.FullName,
            Specialty = d.Specialty,
            ClinicAddress = d.ClinicAddress,
            ConsultationFee = d.ConsultationFee
        });
    }

    public async Task<int?> GetDoctorIdByUserIdAsync(string userId)
    {
        var found = await _unitOfWork.Doctors.FindAsync(d => d.UserId == userId);
        return found.FirstOrDefault()?.Id;
    }

    public async Task<DoctorDto?> GetByIdAsync(int doctorId)
    {
        var found = await _unitOfWork.Doctors.FindAsync(d => d.Id == doctorId, d => d.User);
        var doctor = found.FirstOrDefault();
        return doctor == null ? null : ToDto(doctor);
    }

    public async Task<ServiceResult> UpdateProfileAsync(int doctorId, string specialty, string clinicAddress, decimal consultationFee)
    {
        specialty = (specialty ?? "").Trim();
        clinicAddress = (clinicAddress ?? "").Trim();

        if (specialty.Length == 0)
            return ServiceResult.Fail("Specialty is required.");
        if (specialty.Length > 100)
            return ServiceResult.Fail("Specialty must be 100 characters or fewer.");
        if (clinicAddress.Length == 0)
            return ServiceResult.Fail("Clinic address is required.");
        if (clinicAddress.Length > 300)
            return ServiceResult.Fail("Clinic address must be 300 characters or fewer.");
        if (consultationFee < 0 || consultationFee > 100000)
            return ServiceResult.Fail("Enter a valid consultation fee.");

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
        if (doctor == null)
            return ServiceResult.Fail("Doctor not found.");

        doctor.Specialty = specialty;
        doctor.ClinicAddress = clinicAddress;
        doctor.ConsultationFee = consultationFee;
        _unitOfWork.Doctors.Update(doctor);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        var doctors = await _unitOfWork.Doctors.FindAsync(d => true, d => d.User);
        return doctors
            .OrderBy(d => d.IsApproved)          // pending first
            .ThenBy(d => d.User.FullName)
            .Select(ToDto)
            .ToList();
    }

    public async Task<ServiceResult> SetApprovalAsync(int doctorId, bool approved)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
        if (doctor == null)
            return ServiceResult.Fail("Doctor not found.");

        // A doctor can only be listed for booking once the profile is complete.
        if (approved && (string.IsNullOrWhiteSpace(doctor.Specialty) || string.IsNullOrWhiteSpace(doctor.ClinicAddress)))
            return ServiceResult.Fail("This doctor hasn't completed the profile yet (specialty and clinic address).");

        doctor.IsApproved = approved;
        _unitOfWork.Doctors.Update(doctor);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    private static DoctorDto ToDto(MediCare.Data.Entities.DoctorProfile d) => new()
    {
        Id = d.Id,
        FullName = d.User.FullName,
        Email = d.User.Email ?? string.Empty,
        Specialty = d.Specialty,
        ClinicAddress = d.ClinicAddress,
        ConsultationFee = d.ConsultationFee,
        IsApproved = d.IsApproved
    };
}