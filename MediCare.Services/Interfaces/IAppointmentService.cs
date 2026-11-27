using MediCare.Data.Enums;
using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>
/// The appointment lifecycle: free times, booking, the doctor's decisions and the patient's changes.
/// Every state change sends the matching notification through <see cref="INotificationService"/>.
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// Free 30-minute start times for a doctor on a date: inside the doctor's weekly slots,
    /// not already booked, not on a day off and not in the past. Empty if the doctor isn't approved.
    /// </summary>
    Task<IEnumerable<TimeOnly>> GetAvailableTimesAsync(int doctorId, DateOnly date);

    /// <summary>Books a <c>Pending</c> appointment and notifies the doctor.</summary>
    /// <param name="patientId">Identity user id of the patient.</param>
    /// <param name="doctorId">Id of the doctor's profile.</param>
    /// <param name="date">Appointment date.</param>
    /// <param name="time">Start time; must be one of <see cref="GetAvailableTimesAsync"/>.</param>
    /// <param name="reason">Optional reason for the visit (max 500 characters).</param>
    /// <param name="type">Visit type; its rule and price come from <c>AppointmentFactory</c>.</param>
    /// <returns>Fails when the time isn't free or the visit type's rule isn't met.</returns>
    Task<ServiceResult> BookAsync(string patientId, int doctorId, DateOnly date, TimeOnly time, string? reason,
        AppointmentType type = AppointmentType.FirstVisit);

    /// <summary>The doctor confirms one of their pending appointments; the patient is notified.</summary>
    Task<ServiceResult> ConfirmAsync(int doctorId, int appointmentId);

    /// <summary>The doctor rejects one of their pending appointments; the patient is notified.</summary>
    Task<ServiceResult> RejectAsync(int doctorId, int appointmentId);

    /// <summary>The doctor marks a confirmed appointment as completed (it then counts towards revenue).</summary>
    Task<ServiceResult> CompleteAsync(int doctorId, int appointmentId);

    /// <summary>The doctor marks a confirmed appointment as a no-show.</summary>
    Task<ServiceResult> MarkNoShowAsync(int doctorId, int appointmentId);

    /// <summary>All appointments of a doctor, newest first.</summary>
    Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId);

    /// <summary>The patient cancels their own pending or confirmed appointment; the doctor is notified.</summary>
    Task<ServiceResult> CancelAsync(string patientId, int appointmentId);

    /// <summary>The patient moves their own appointment to another free time; it goes back to <c>Pending</c>.</summary>
    Task<ServiceResult> RescheduleAsync(string patientId, int appointmentId, DateOnly newDate, TimeOnly newTime);

    /// <summary>All appointments of a patient, newest first.</summary>
    Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(string patientId);
}
