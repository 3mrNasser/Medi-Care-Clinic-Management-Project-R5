using MediCare.Services.Common;

namespace MediCare.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<TimeOnly>> GetAvailableTimesAsync(int doctorId, DateOnly date);
    Task<ServiceResult> BookAsync(string patientId, int doctorId, DateOnly date, TimeOnly time, string? reason);
}