using MediCare.Data.Enums;

namespace MediCare.Services.Notifications;

// Everything a notification message may need about one appointment.
public record AppointmentNotificationContext(
    int AppointmentId,
    string PatientUserId,
    string PatientName,
    string? PatientEmail,
    string? PatientPhone,
    string DoctorUserId,
    string DoctorName,
    DateOnly Date,
    TimeOnly Time,
    string? ClinicAddress,
    bool PatientWantsEmail = true,
    bool PatientWantsSms = true)
{
    public string When => $"{Date:ddd d MMM} at {Time:HH:mm}";
}

// One kind of appointment notification: who receives it, what it says, where it links, and which channels it uses.
// New kinds are added as new classes + one line in NotificationFactory (Factory Pattern).
public interface IAppointmentNotification
{
    NotificationType Type { get; }
    string RecipientUserId(AppointmentNotificationContext c);
    string Title(AppointmentNotificationContext c);
    string Message(AppointmentNotificationContext c);
    string Link { get; }
    bool SendEmail { get; }   // also email the recipient
    bool SendSms { get; }     // also text the recipient (if a phone number exists)
}

// ----- To the doctor -----

public sealed class AppointmentBookedNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentBooked;
    public string RecipientUserId(AppointmentNotificationContext c) => c.DoctorUserId;
    public string Title(AppointmentNotificationContext c) => "New appointment request";
    public string Message(AppointmentNotificationContext c) => $"{c.PatientName} asked for {c.When}. Confirm or reject it.";
    public string Link => "/DoctorAppointments";
    public bool SendEmail => false;
    public bool SendSms => false;
}

public sealed class AppointmentRescheduledNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentRescheduled;
    public string RecipientUserId(AppointmentNotificationContext c) => c.DoctorUserId;
    public string Title(AppointmentNotificationContext c) => "Appointment rescheduled";
    public string Message(AppointmentNotificationContext c) => $"{c.PatientName} moved their appointment to {c.When}. It needs your confirmation.";
    public string Link => "/DoctorAppointments";
    public bool SendEmail => false;
    public bool SendSms => false;
}

public sealed class AppointmentCancelledNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentCancelled;
    public string RecipientUserId(AppointmentNotificationContext c) => c.DoctorUserId;
    public string Title(AppointmentNotificationContext c) => "Appointment cancelled";
    public string Message(AppointmentNotificationContext c) => $"{c.PatientName} cancelled the appointment on {c.When}.";
    public string Link => "/DoctorAppointments";
    public bool SendEmail => false;
    public bool SendSms => false;
}

// ----- To the patient -----

public sealed class AppointmentConfirmedNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentConfirmed;
    public string RecipientUserId(AppointmentNotificationContext c) => c.PatientUserId;
    public string Title(AppointmentNotificationContext c) => "Appointment confirmed";
    public string Message(AppointmentNotificationContext c) => $"{c.DoctorName} confirmed your appointment on {c.When}.";
    public string Link => "/MyAppointments";
    public bool SendEmail => true;
    public bool SendSms => false;
}

public sealed class AppointmentRejectedNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentRejected;
    public string RecipientUserId(AppointmentNotificationContext c) => c.PatientUserId;
    public string Title(AppointmentNotificationContext c) => "Appointment not accepted";
    public string Message(AppointmentNotificationContext c) => $"{c.DoctorName} couldn't accept your appointment on {c.When}. Please book another time.";
    public string Link => "/Doctors";
    public bool SendEmail => true;
    public bool SendSms => false;
}

public sealed class AppointmentCompletedNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentCompleted;
    public string RecipientUserId(AppointmentNotificationContext c) => c.PatientUserId;
    public string Title(AppointmentNotificationContext c) => "Visit completed";
    public string Message(AppointmentNotificationContext c) => $"Your visit with {c.DoctorName} is done. Your prescription and medical record will appear in your account.";
    public string Link => "/MyMedicalRecords";
    public bool SendEmail => false;
    public bool SendSms => false;
}

public sealed class AppointmentReminderNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentReminder;
    public string RecipientUserId(AppointmentNotificationContext c) => c.PatientUserId;
    public string Title(AppointmentNotificationContext c) => "Appointment reminder";
    public string Message(AppointmentNotificationContext c) =>
        $"Reminder: you see {c.DoctorName} on {c.When}" + (string.IsNullOrWhiteSpace(c.ClinicAddress) ? "." : $" at {c.ClinicAddress}.");
    public string Link => "/MyAppointments";
    public bool SendEmail => true;
    public bool SendSms => true;
}

public sealed class AppointmentCancelledByClinicNotification : IAppointmentNotification
{
    public NotificationType Type => NotificationType.AppointmentCancelledByClinic;
    public string RecipientUserId(AppointmentNotificationContext c) => c.PatientUserId;
    public string Title(AppointmentNotificationContext c) => "Appointment cancelled by the clinic";
    public string Message(AppointmentNotificationContext c) => $"Your appointment with {c.DoctorName} on {c.When} was cancelled by the clinic. Please book another time.";
    public string Link => "/Doctors";
    public bool SendEmail => true;
    public bool SendSms => true;
}

// Factory Pattern: creates the right notification object for each event, so callers never switch on the type themselves.
public static class NotificationFactory
{
    public static IAppointmentNotification Create(NotificationType type) => type switch
    {
        NotificationType.AppointmentBooked => new AppointmentBookedNotification(),
        NotificationType.AppointmentRescheduled => new AppointmentRescheduledNotification(),
        NotificationType.AppointmentCancelled => new AppointmentCancelledNotification(),
        NotificationType.AppointmentConfirmed => new AppointmentConfirmedNotification(),
        NotificationType.AppointmentRejected => new AppointmentRejectedNotification(),
        NotificationType.AppointmentCompleted => new AppointmentCompletedNotification(),
        NotificationType.AppointmentReminder => new AppointmentReminderNotification(),
        NotificationType.AppointmentCancelledByClinic => new AppointmentCancelledByClinicNotification(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown notification type.")
    };
}

// When the 24-hour reminder is due (kept pure so it can be unit-tested).
public static class ReminderRules
{
    public static readonly TimeSpan Window = TimeSpan.FromHours(24);

    public static bool IsDue(DateOnly date, TimeOnly time, DateTime now)
    {
        var start = date.ToDateTime(time);
        return start > now && start <= now.Add(Window);
    }
}
