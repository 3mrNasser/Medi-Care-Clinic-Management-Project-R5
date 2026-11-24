namespace MediCare.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<TimeOnly>> GetAvailableTimesAsync(int doctorId, DateOnly date);
}