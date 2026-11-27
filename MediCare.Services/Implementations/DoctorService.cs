using MediCare.Data.Repositories.Interfaces;
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
}