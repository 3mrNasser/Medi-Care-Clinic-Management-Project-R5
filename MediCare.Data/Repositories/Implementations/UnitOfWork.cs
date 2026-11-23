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
    }

    public IGenericRepository<DoctorProfile> Doctors { get; }
    public IGenericRepository<AvailabilitySlot> Slots { get; }
    public IGenericRepository<Appointment> Appointments { get; }
    public IGenericRepository<Prescription> Prescriptions { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}