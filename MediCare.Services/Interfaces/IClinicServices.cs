using MediCare.Data.Enums;
using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface ISpecializationService
{
    Task<IReadOnlyList<SpecializationDto>> GetAllAsync(bool includeHidden);
    Task<IReadOnlyList<string>> GetActiveNamesAsync();
    Task<ServiceResult> AddAsync(string name, string? description);
    Task<ServiceResult> UpdateAsync(int id, string name, string? description, bool isActive);
}

public interface IPatientProfileService
{
    Task<PatientProfileDto?> GetAsync(string patientId);
    Task<ServiceResult> SaveAsync(string patientId, PatientProfileDto input);
    // Doctors can open a patient's profile only if the patient booked with them.
    Task<bool> DoctorCanViewAsync(int doctorId, string patientId);
    Task<IReadOnlyList<AppointmentDto>> GetHistoryWithDoctorAsync(int doctorId, string patientId);
}

// Where uploaded files are kept (implemented on disk in MediCare.Web; could be Azure Blob Storage later).
public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string extension);
    Task<Stream?> OpenReadAsync(string storedFileName);
    Task DeleteAsync(string storedFileName);
}

public interface IMedicalDocumentService
{
    Task<IReadOnlyList<MedicalDocumentDto>> GetForPatientAsync(string patientId, string viewerUserId);
    // uploaderDoctorId: set when a doctor uploads for their patient; null when the patient uploads.
    Task<ServiceResult> UploadAsync(string patientId, string uploaderUserId, int? uploaderDoctorId,
        string title, string category, string fileName, long size, Stream content);
    // Returns the file only if the viewer may see it (the patient, or a doctor the patient booked with).
    Task<(MedicalDocumentDto Document, Stream Content)?> OpenAsync(int documentId, string viewerUserId, int? viewerDoctorId);
    Task<ServiceResult> DeleteAsync(int documentId, string userId);
}

public interface ITimeOffService
{
    Task<IReadOnlyList<TimeOffDto>> GetUpcomingAsync(int doctorId);
    Task<ServiceResult> AddAsync(int doctorId, DateOnly date, string? reason);
    Task<ServiceResult> RemoveAsync(int doctorId, int timeOffId);
}

public interface IAdminService
{
    Task<IReadOnlyList<AdminPatientDto>> GetPatientsAsync(string? search);
    Task<ServiceResult> SetPatientLockedAsync(string patientId, bool locked);
    Task<IReadOnlyList<AdminAppointmentDto>> GetAppointmentsAsync(AppointmentStatus? status, DateOnly? from, DateOnly? to, string? search);
    Task<ServiceResult> CancelAppointmentAsync(int appointmentId);
}

public interface IReportService
{
    Task<AnalyticsDto> GetAnalyticsAsync(DateOnly from, DateOnly to);
}
