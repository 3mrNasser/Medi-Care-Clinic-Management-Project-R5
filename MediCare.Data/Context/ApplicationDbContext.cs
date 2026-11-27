using MediCare.Data.Entities;
using MediCare.Data.Enums;
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
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();
    public DbSet<DoctorTimeOff> DoctorTimeOffs => Set<DoctorTimeOff>();

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
            e.Property(a => a.Type).HasConversion<string>().HasMaxLength(20).HasDefaultValue(AppointmentType.FirstVisit);
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

        builder.Entity<Notification>(e =>
        {
            e.Property(n => n.Type).HasConversion<string>().HasMaxLength(40);
            e.Property(n => n.Title).HasMaxLength(150);
            e.Property(n => n.Message).HasMaxLength(500);
            e.Property(n => n.Link).HasMaxLength(300);

            // Unread count and the list are always read per user, newest first.
            e.HasIndex(n => new { n.UserId, n.IsRead });

            e.HasOne(n => n.User).WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.EmailNotificationsEnabled).HasDefaultValue(true);
            e.Property(u => u.SmsNotificationsEnabled).HasDefaultValue(true);
        });

        builder.Entity<Specialization>(e =>
        {
            e.Property(s => s.Name).HasMaxLength(100);
            e.Property(s => s.Description).HasMaxLength(300);
            e.HasIndex(s => s.Name).IsUnique();
        });

        builder.Entity<PatientProfile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();
            e.Property(p => p.Gender).HasMaxLength(10);
            e.Property(p => p.BloodType).HasMaxLength(5);
            e.Property(p => p.Address).HasMaxLength(250);
            e.Property(p => p.Allergies).HasMaxLength(500);
            e.Property(p => p.ChronicConditions).HasMaxLength(500);
            e.Property(p => p.CurrentMedications).HasMaxLength(500);
            e.Property(p => p.MedicalHistoryNotes).HasMaxLength(2000);
            e.Property(p => p.EmergencyContactName).HasMaxLength(100);
            e.Property(p => p.EmergencyContactPhone).HasMaxLength(30);
            e.HasOne(p => p.User).WithMany()
                .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MedicalDocument>(e =>
        {
            e.Property(d => d.Title).HasMaxLength(150);
            e.Property(d => d.Category).HasMaxLength(30);
            e.Property(d => d.OriginalFileName).HasMaxLength(255);
            e.Property(d => d.StoredFileName).HasMaxLength(100);
            e.Property(d => d.ContentType).HasMaxLength(100);
            e.Property(d => d.UploadedByUserId).HasMaxLength(450);
            e.HasIndex(d => d.PatientId);
            e.HasOne(d => d.Patient).WithMany()
                .HasForeignKey(d => d.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DoctorTimeOff>(e =>
        {
            e.Property(t => t.Reason).HasMaxLength(200);
            e.HasIndex(t => new { t.DoctorProfileId, t.Date }).IsUnique();
            e.HasOne(t => t.DoctorProfile).WithMany()
                .HasForeignKey(t => t.DoctorProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MedicalRecord>(e =>
        {
            e.Property(m => m.Diagnosis).HasMaxLength(500);
            e.Property(m => m.Notes).HasMaxLength(2000);

            e.HasIndex(m => m.AppointmentId).IsUnique();

            e.HasOne(m => m.Appointment).WithMany()
                .HasForeignKey(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}