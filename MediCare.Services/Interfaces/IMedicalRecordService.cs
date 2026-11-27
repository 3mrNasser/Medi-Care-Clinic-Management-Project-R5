using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IMedicalRecordService
{
    Task<ServiceResult> SaveAsync(int doctorId, int appointmentId, string diagnosis, string? notes);
    Task<MedicalRecordDto?> GetForDoctorAsync(int doctorId, int appointmentId);
    Task<IEnumerable<MedicalRecordDto>> GetForPatientAsync(string patientId);
}