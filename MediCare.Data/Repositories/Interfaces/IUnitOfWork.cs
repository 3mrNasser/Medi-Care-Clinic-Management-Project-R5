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

    Task<int> SaveChangesAsync();
}