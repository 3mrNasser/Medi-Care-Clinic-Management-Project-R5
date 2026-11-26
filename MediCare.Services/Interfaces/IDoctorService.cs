using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<DoctorDto>> SearchAsync(string? specialty, string? name, string? location);
}