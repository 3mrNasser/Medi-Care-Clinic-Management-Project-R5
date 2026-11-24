namespace MediCare.Data.Entities;

public class AvailabilitySlot
{
    public int Id { get; set; }
    public int DoctorProfileId { get; set; }
    public DoctorProfile DoctorProfile { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}