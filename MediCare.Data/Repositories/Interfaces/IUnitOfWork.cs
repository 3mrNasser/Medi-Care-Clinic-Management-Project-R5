using MediCare.Data.Entities;

namespace MediCare.Data.Repositories.Interfaces;

/// <summary>
/// Unit of Work Pattern: one repository per entity, all sharing one <c>DbContext</c>,
/// so a service's changes are saved together in one transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Doctor profiles.</summary>
    IGenericRepository<DoctorProfile> Doctors { get; }
    /// <summary>Doctors' weekly availability slots.</summary>
    IGenericRepository<AvailabilitySlot> Slots { get; }
    /// <summary>Appointments.</summary>
    IGenericRepository<Appointment> Appointments { get; }
    /// <summary>Prescriptions.</summary>
    IGenericRepository<Prescription> Prescriptions { get; }
    /// <summary>Medical records (diagnosis and notes).</summary>
    IGenericRepository<MedicalRecord> MedicalRecords { get; }
    /// <summary>In-app notifications.</summary>
    IGenericRepository<Notification> Notifications { get; }
    /// <summary>Medical specialties.</summary>
    IGenericRepository<Specialization> Specializations { get; }
    /// <summary>Patients' medical profiles.</summary>
    IGenericRepository<PatientProfile> PatientProfiles { get; }
    /// <summary>Uploaded medical documents.</summary>
    IGenericRepository<MedicalDocument> MedicalDocuments { get; }
    /// <summary>Doctors' days off.</summary>
    IGenericRepository<DoctorTimeOff> TimeOffs { get; }
    /// <summary>Identity users (read-only use; accounts are changed through <c>UserManager</c>).</summary>
    IGenericRepository<ApplicationUser> Users { get; }

    /// <summary>Saves every pending change in one transaction and returns the number of rows written.</summary>
    Task<int> SaveChangesAsync();
}
