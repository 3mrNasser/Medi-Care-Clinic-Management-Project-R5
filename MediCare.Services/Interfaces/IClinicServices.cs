using MediCare.Data.Enums;
using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>The list of medical specialties the admin manages and doctors choose from.</summary>
public interface ISpecializationService
{
    /// <summary>All specialties; hidden ones only when <paramref name="includeHidden"/> is true.</summary>
    Task<IReadOnlyList<SpecializationDto>> GetAllAsync(bool includeHidden);

    /// <summary>Names of the visible specialties, for dropdowns and search.</summary>
    Task<IReadOnlyList<string>> GetActiveNamesAsync();

    /// <summary>Admin: adds a specialty (names are unique).</summary>
    Task<ServiceResult> AddAsync(string name, string? description);

    /// <summary>Admin: renames, describes, shows or hides a specialty.</summary>
    Task<ServiceResult> UpdateAsync(int id, string name, string? description, bool isActive);
}

/// <summary>The patient's medical profile (blood type, allergies, conditions, emergency contact).</summary>
public interface IPatientProfileService
{
    /// <summary>The patient's profile, or <c>null</c> if the user doesn't exist.</summary>
    Task<PatientProfileDto?> GetAsync(string patientId);

    /// <summary>Creates or updates the patient's profile.</summary>
    Task<ServiceResult> SaveAsync(string patientId, PatientProfileDto input);

    /// <summary>Doctors can open a patient's profile only if the patient booked with them.</summary>
    Task<bool> DoctorCanViewAsync(int doctorId, string patientId);

    /// <summary>The patient's appointments with this doctor, newest first.</summary>
    Task<IReadOnlyList<AppointmentDto>> GetHistoryWithDoctorAsync(int doctorId, string patientId);
}

/// <summary>Where uploaded files are kept (implemented on disk in MediCare.Web; could be Azure Blob Storage later).</summary>
public interface IFileStorage
{
    /// <summary>Stores the content under a new random name and returns that name.</summary>
    Task<string> SaveAsync(Stream content, string extension);

    /// <summary>Opens a stored file, or returns <c>null</c> if it's missing.</summary>
    Task<Stream?> OpenReadAsync(string storedFileName);

    /// <summary>Deletes a stored file (no error if it's already gone).</summary>
    Task DeleteAsync(string storedFileName);
}

/// <summary>Medical documents (lab results, scans, reports) uploaded to a patient's file.</summary>
public interface IMedicalDocumentService
{
    /// <summary>The patient's documents, with who uploaded each one worded for <paramref name="viewerUserId"/>.</summary>
    Task<IReadOnlyList<MedicalDocumentDto>> GetForPatientAsync(string patientId, string viewerUserId);

    /// <summary>
    /// Uploads a PDF, JPG or PNG (up to 10 MB) to the patient's file.
    /// <paramref name="uploaderDoctorId"/> is set when a doctor uploads for their patient; <c>null</c> when the patient uploads.
    /// </summary>
    Task<ServiceResult> UploadAsync(string patientId, string uploaderUserId, int? uploaderDoctorId,
        string title, string category, string fileName, long size, Stream content);

    /// <summary>Returns the file only if the viewer may see it (the patient, or a doctor the patient booked with).</summary>
    Task<(MedicalDocumentDto Document, Stream Content)?> OpenAsync(int documentId, string viewerUserId, int? viewerDoctorId);

    /// <summary>Deletes a document; the patient or whoever uploaded it can.</summary>
    Task<ServiceResult> DeleteAsync(int documentId, string userId);
}

/// <summary>A doctor's days off; no times are offered on those days.</summary>
public interface ITimeOffService
{
    /// <summary>The doctor's days off from today on.</summary>
    Task<IReadOnlyList<TimeOffDto>> GetUpcomingAsync(int doctorId);

    /// <summary>Adds a day off. Fails for past days or days that already have active appointments.</summary>
    Task<ServiceResult> AddAsync(int doctorId, DateOnly date, string? reason);

    /// <summary>Removes one of the doctor's days off.</summary>
    Task<ServiceResult> RemoveAsync(int doctorId, int timeOffId);
}

/// <summary>Admin tools for patients and appointments.</summary>
public interface IAdminService
{
    /// <summary>Patients, optionally filtered by name or email.</summary>
    Task<IReadOnlyList<AdminPatientDto>> GetPatientsAsync(string? search);

    /// <summary>Locks a patient's account (they can't sign in) or unlocks it.</summary>
    Task<ServiceResult> SetPatientLockedAsync(string patientId, bool locked);

    /// <summary>All appointments, filtered by status, date range and patient / doctor name. Fee uses the visit type's price.</summary>
    Task<IReadOnlyList<AdminAppointmentDto>> GetAppointmentsAsync(AppointmentStatus? status, DateOnly? from, DateOnly? to, string? search);

    /// <summary>Cancels a pending or confirmed appointment; patient and doctor are notified.</summary>
    Task<ServiceResult> CancelAppointmentAsync(int appointmentId);
}

/// <summary>Analytics for the admin dashboard and the PDF / Excel reports.</summary>
public interface IReportService
{
    /// <summary>Counts, revenue (completed visits, priced by visit type), monthly trends and top specialties for a date range.</summary>
    Task<AnalyticsDto> GetAnalyticsAsync(DateOnly from, DateOnly to);
}
