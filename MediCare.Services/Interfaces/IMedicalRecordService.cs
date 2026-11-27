using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>The diagnosis and notes a doctor writes for a completed appointment.</summary>
public interface IMedicalRecordService
{
    /// <summary>Creates or updates the record of one of the doctor's completed appointments.</summary>
    Task<ServiceResult> SaveAsync(int doctorId, int appointmentId, string diagnosis, string? notes);

    /// <summary>The record of one of the doctor's appointments, or <c>null</c> if none / not theirs.</summary>
    Task<MedicalRecordDto?> GetForDoctorAsync(int doctorId, int appointmentId);

    /// <summary>All of a patient's records, newest first.</summary>
    Task<IEnumerable<MedicalRecordDto>> GetForPatientAsync(string patientId);
}
