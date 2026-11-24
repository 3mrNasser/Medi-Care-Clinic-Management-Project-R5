using MediCare.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DoctorProfile>(e =>
        {
            e.Property(d => d.Specialty).HasMaxLength(100);
            e.Property(d => d.ClinicAddress).HasMaxLength(250);
            e.Property(d => d.ConsultationFee).HasPrecision(18, 2);
            e.HasIndex(d => d.Specialty);
            e.HasIndex(d => d.UserId).IsUnique();
        });

        builder.Entity<AvailabilitySlot>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_AvailabilitySlot_Times", "[EndTime] > [StartTime]"));
        });

        builder.Entity<Appointment>(e =>
        {
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(a => a.ReasonForVisit).HasMaxLength(500);

            e.HasOne(a => a.Patient).WithMany()
                .HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.DoctorProfile).WithMany()
                .HasForeignKey(a => a.DoctorProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.AvailabilitySlot).WithMany()
                .HasForeignKey(a => a.AvailabilitySlotId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(a => new { a.DoctorProfileId, a.AppointmentDate, a.StartTime })
                .IsUnique()
                .HasFilter("[Status] IN ('Pending', 'Confirmed')");
        });

        builder.Entity<Prescription>(e =>
        {
            e.Property(p => p.MedicationName).HasMaxLength(150);
            e.Property(p => p.Dosage).HasMaxLength(100);
            e.Property(p => p.Frequency).HasMaxLength(100);
            e.ToTable(t => t.HasCheckConstraint("CK_Prescription_Duration", "[DurationDays] > 0"));
        });
    }
}