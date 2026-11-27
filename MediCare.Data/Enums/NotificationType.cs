namespace MediCare.Data.Enums;

// What happened to an appointment. NotificationFactory turns each type into the right message for the right person.
public enum NotificationType
{
    AppointmentBooked,      // → doctor
    AppointmentRescheduled, // → doctor
    AppointmentCancelled,   // → doctor
    AppointmentConfirmed,   // → patient
    AppointmentRejected,    // → patient
    AppointmentCompleted,   // → patient
    AppointmentReminder,    // → patient, ~24 hours before
    AppointmentCancelledByClinic // → patient, when the admin cancels
}
