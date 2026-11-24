using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Common;
using MediCare.Services.Interfaces;

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
                if (!bookedTimes.Contains(time))
                    result.Add(time);

                current += step;
            }
        }

        return result.OrderBy(t => t);
    }
}