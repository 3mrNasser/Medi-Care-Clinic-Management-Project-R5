using MediCare.Data.Entities;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Common;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Implementations;

public class AvailabilityService : IAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;

    public AvailabilityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AvailabilitySlotDto>> GetByDoctorAsync(int doctorId)
    {
        var slots = await _unitOfWork.Slots.FindAsync(s => s.DoctorProfileId == doctorId);

        return slots
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new AvailabilitySlotDto
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            });
    }

    public async Task<ServiceResult> AddAsync(int doctorId, DayOfWeek day, TimeOnly start, TimeOnly end)
    {
        if (end <= start)
            return ServiceResult.Fail("End time must be after start time.");

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
        if (doctor is null)
            return ServiceResult.Fail("Doctor not found.");

        var overlapping = await _unitOfWork.Slots.FindAsync(s =>
            s.DoctorProfileId == doctorId
            && s.DayOfWeek == day
            && start < s.EndTime
            && end > s.StartTime);

        if (overlapping.Any())
            return ServiceResult.Fail("This time overlaps with an existing slot.");

        await _unitOfWork.Slots.AddAsync(new AvailabilitySlot
        {
            DoctorProfileId = doctorId,
            DayOfWeek = day,
            StartTime = start,
            EndTime = end
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveAsync(int doctorId, int slotId)
    {
        var slot = await _unitOfWork.Slots.GetByIdAsync(slotId);
        if (slot is null || slot.DoctorProfileId != doctorId)
            return ServiceResult.Fail("Slot not found.");

        var appointments = await _unitOfWork.Appointments.FindAsync(a => a.AvailabilitySlotId == slotId);
        if (appointments.Any())
            return ServiceResult.Fail("This slot already has appointments and cannot be deleted.");

        _unitOfWork.Slots.Remove(slot);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}