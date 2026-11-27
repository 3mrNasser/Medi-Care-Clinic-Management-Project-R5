using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>Medications a doctor prescribes for a completed appointment.</summary>
public interface IPrescriptionService
{
    /// <summary>Adds a medication to one of the doctor's completed appointments.</summary>
    /// <param name="doctorId">The prescribing doctor; must own the appointment.</param>
    /// <param name="appointmentId">A completed appointment.</param>
    /// <param name="medicationName">Medication name.</param>
    /// <param name="dosage">Dose, e.g. "500 mg".</param>
    /// <param name="frequency">How often, e.g. "twice a day".</param>
    /// <param name="durationDays">How many days (greater than 0).</param>
    Task<ServiceResult> AddAsync(
        int doctorId, int appointmentId,
        string medicationName, string dosage, string frequency, int durationDays);

    /// <summary>Medications of one of the doctor's appointments.</summary>
    Task<IEnumerable<PrescriptionDto>> GetForAppointmentAsync(int doctorId, int appointmentId);

    /// <summary>A patient's prescriptions grouped by appointment, newest first.</summary>
    Task<IEnumerable<PrescriptionGroupDto>> GetForPatientAsync(string patientId);
}
