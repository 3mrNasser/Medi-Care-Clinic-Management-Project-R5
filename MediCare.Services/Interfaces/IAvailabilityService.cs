using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IAvailabilityService
{
    Task<IEnumerable<AvailabilitySlotDto>> GetByDoctorAsync(int doctorId);
    Task<ServiceResult> AddAsync(int doctorId, DayOfWeek day, TimeOnly start, TimeOnly end);
    Task<ServiceResult> RemoveAsync(int doctorId, int slotId);
}