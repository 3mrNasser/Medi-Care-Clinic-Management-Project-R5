using MediCare.Data.Entities;

namespace MediCare.Data.Repositories.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<DoctorProfile> Doctors { get; }
    IGenericRepository<AvailabilitySlot> Slots { get; }
    IGenericRepository<Appointment> Appointments { get; }
    IGenericRepository<Prescription> Prescriptions { get; }
    IGenericRepository<MedicalRecord> MedicalRecords { get; }
    IGenericRepository<Notification> Notifications { get; }
    IGenericRepository<Specialization> Specializations { get; }
    IGenericRepository<PatientProfile> PatientProfiles { get; }
    IGenericRepository<MedicalDocument> MedicalDocuments { get; }
    IGenericRepository<DoctorTimeOff> TimeOffs { get; }
    IGenericRepository<ApplicationUser> Users { get; }

    Task<int> SaveChangesAsync();
}