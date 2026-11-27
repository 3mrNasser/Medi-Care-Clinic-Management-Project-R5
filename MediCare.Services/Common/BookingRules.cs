namespace MediCare.Services.Common;

/// <summary>Fixed booking rules shared by the services.</summary>
public static class BookingRules
{
    /// <summary>Length of one appointment in minutes; free times are cut from slots in these steps.</summary>
    public const int AppointmentMinutes = 30;
}
