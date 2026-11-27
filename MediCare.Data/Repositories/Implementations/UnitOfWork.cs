using MediCare.Data.Context;
using MediCare.Data.Entities;
using MediCare.Data.Repositories.Interfaces;

namespace MediCare.Data.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Doctors = new GenericRepository<DoctorProfile>(context);
        Slots = new GenericRepository<AvailabilitySlot>(context);
        Appointments = new GenericRepository<Appointment>(context);
        Prescriptions = new GenericRepository<Prescription>(context);
        MedicalRecords = new GenericRepository<MedicalRecord>(context);
        Notifications = new GenericRepository<Notification>(context);
        Specializations = new GenericRepository<Specialization>(context);
        PatientProfiles = new GenericRepository<PatientProfile>(context);
        MedicalDocuments = new GenericRepository<MedicalDocument>(context);
        TimeOffs = new GenericRepository<DoctorTimeOff>(context);
        Users = new GenericRepository<ApplicationUser>(context);
    }

    public IGenericRepository<DoctorProfile> Doctors { get; }
    public IGenericRepository<AvailabilitySlot> Slots { get; }
    public IGenericRepository<Appointment> Appointments { get; }
    public IGenericRepository<Prescription> Prescriptions { get; }
    public IGenericRepository<MedicalRecord> MedicalRecords { get; }
    public IGenericRepository<Notification> Notifications { get; }
    public IGenericRepository<Specialization> Specializations { get; }
    public IGenericRepository<PatientProfile> PatientProfiles { get; }
    public IGenericRepository<MedicalDocument> MedicalDocuments { get; }
    public IGenericRepository<DoctorTimeOff> TimeOffs { get; }
    public IGenericRepository<ApplicationUser> Users { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}