using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Common;
using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace MediCare.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TimeOnly>> GetAvailableTimesAsync(int doctorId, DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (date < today)
            return Enumerable.Empty<TimeOnly>();

        var doctors = await _unitOfWork.Doctors.FindAsync(d => d.Id == doctorId && d.IsApproved);
        if (!doctors.Any())
            return Enumerable.Empty<TimeOnly>();

        var slots = await _unitOfWork.Slots.FindAsync(s =>
            s.DoctorProfileId == doctorId && s.DayOfWeek == date.DayOfWeek);

        var booked = await _unitOfWork.Appointments.FindAsync(a =>
            a.DoctorProfileId == doctorId
            && a.AppointmentDate == date
            && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

        var bookedTimes = booked.Select(a => a.StartTime).ToHashSet();

        var step = TimeSpan.FromMinutes(BookingRules.AppointmentMinutes);
        var result = new List<TimeOnly>();

        foreach (var slot in slots)
        {
            var current = slot.StartTime.ToTimeSpan();
            var end = slot.EndTime.ToTimeSpan();

            while (current + step <= end)
            {
                var time = TimeOnly.FromTimeSpan(current);
                var isPast = date == today && time <= TimeOnly.FromDateTime(DateTime.Now);
                if (!bookedTimes.Contains(time) && !isPast)
                    result.Add(time);

                current += step;
            }
        }

        return result.OrderBy(t => t);
    }

    public async Task<ServiceResult> BookAsync(
    string patientId, int doctorId, DateOnly date, TimeOnly time, string? reason)
    {
        if (reason is { Length: > 500 })
            return ServiceResult.Fail("Reason is too long (maximum 500 characters).");

        var available = await GetAvailableTimesAsync(doctorId, date);
        if (!available.Contains(time))
            return ServiceResult.Fail("This time is not available.");

        var step = TimeSpan.FromMinutes(BookingRules.AppointmentMinutes);
        var slots = await _unitOfWork.Slots.FindAsync(s =>
            s.DoctorProfileId == doctorId && s.DayOfWeek == date.DayOfWeek);

        var slot = slots.FirstOrDefault(s =>
            time >= s.StartTime && time.ToTimeSpan() + step <= s.EndTime.ToTimeSpan());

        if (slot is null)
            return ServiceResult.Fail("This time is not available.");

        await _unitOfWork.Appointments.AddAsync(new Appointment
        {
            PatientId = patientId,
            DoctorProfileId = doctorId,
            AvailabilitySlotId = slot.Id,
            AppointmentDate = date,
            StartTime = time,
            Status = AppointmentStatus.Pending,
            ReasonForVisit = reason
        });
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            var stillAvailable = await GetAvailableTimesAsync(doctorId, date);
            if (!stillAvailable.Contains(time))
                return ServiceResult.Fail("This time was just booked by someone else.");

            throw;
        }

        return ServiceResult.Ok();
    }
}