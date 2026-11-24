using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<TimeOnly>> GetAvailableTimesAsync(int doctorId, DateOnly date);
    Task<ServiceResult> BookAsync(string patientId, int doctorId, DateOnly date, TimeOnly time, string? reason);
    Task<ServiceResult> ConfirmAsync(int doctorId, int appointmentId);
    Task<ServiceResult> RejectAsync(int doctorId, int appointmentId);
    Task<ServiceResult> CompleteAsync(int doctorId, int appointmentId);
    Task<ServiceResult> MarkNoShowAsync(int doctorId, int appointmentId);
    Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId);
    Task<ServiceResult> CancelAsync(string patientId, int appointmentId);
    Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(string patientId);
}