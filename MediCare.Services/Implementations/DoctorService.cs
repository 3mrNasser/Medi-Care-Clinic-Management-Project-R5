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

    public async Task<DoctorProfileDto?> GetProfileAsync(int doctorId)
    {
        var found = await _unitOfWork.Doctors.FindAsync(d => d.Id == doctorId, d => d.User);
        var doctor = found.FirstOrDefault();
        if (doctor is null)
            return null;

        return new DoctorProfileDto
        {
            FullName = doctor.User.FullName,
            Specialty = doctor.Specialty,
            ClinicAddress = doctor.ClinicAddress,
            ConsultationFee = doctor.ConsultationFee,
            IsApproved = doctor.IsApproved
        };
    }

    public async Task<ServiceResult> UpdateProfileAsync(
        int doctorId, string specialty, string clinicAddress, decimal consultationFee)
    {
        specialty = specialty?.Trim() ?? string.Empty;
        clinicAddress = clinicAddress?.Trim() ?? string.Empty;

        if (!Specialties.IsValid(specialty))
            return ServiceResult.Fail("Please choose a specialty from the list.");

        if (clinicAddress.Length == 0)
            return ServiceResult.Fail("Clinic address is required.");

        if (clinicAddress.Length > 250)
            return ServiceResult.Fail("Clinic address is too long (maximum 250 characters).");

        if (consultationFee <= 0 || consultationFee > 100000)
            return ServiceResult.Fail("Consultation fee must be between 1 and 100,000.");

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
        if (doctor is null)
            return ServiceResult.Fail("Doctor not found.");

        doctor.Specialty = specialty;
        doctor.ClinicAddress = clinicAddress;
        doctor.ConsultationFee = Math.Round(consultationFee, 2);

        _unitOfWork.Doctors.Update(doctor);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}