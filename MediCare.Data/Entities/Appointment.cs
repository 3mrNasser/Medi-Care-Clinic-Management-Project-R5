using MediCare.Data.Enums;

namespace MediCare.Data.Entities;

public class Appointment
{
    public int Id { get; set; }

    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser Patient { get; set; } = null!;

    public int DoctorProfileId { get; set; }
    public DoctorProfile DoctorProfile { get; set; } = null!;

    public int AvailabilitySlotId { get; set; }
    public AvailabilitySlot AvailabilitySlot { get; set; } = null!;

    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? ReasonForVisit { get; set; }

    // Set when the 24-hour reminder was sent, so it's sent only once. Cleared when the appointment is rescheduled.
    public DateTime? ReminderSentAtUtc { get; set; }
}