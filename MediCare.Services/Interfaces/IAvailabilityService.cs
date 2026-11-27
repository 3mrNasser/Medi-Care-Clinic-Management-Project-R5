using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>A doctor's weekly working hours (availability slots) that booking times are cut from.</summary>
public interface IAvailabilityService
{
    /// <summary>The doctor's weekly slots, ordered by day and start time.</summary>
    Task<IEnumerable<AvailabilitySlotDto>> GetByDoctorAsync(int doctorId);

    /// <summary>Adds a weekly slot. Fails if the end isn't after the start or it overlaps another slot that day.</summary>
    Task<ServiceResult> AddAsync(int doctorId, DayOfWeek day, TimeOnly start, TimeOnly end);

    /// <summary>Removes one of the doctor's own slots; fails if it already has appointments.</summary>
    Task<ServiceResult> RemoveAsync(int doctorId, int slotId);
}
