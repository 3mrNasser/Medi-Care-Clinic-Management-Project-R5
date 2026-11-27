using MediCare.Data.Enums;
using MediCare.Services.Notifications;

namespace MediCare.Tests;

// Unit tests: the Factory returns the right notification for every event, sent to the right person.
public class NotificationFactoryTests
{
    private static readonly AppointmentNotificationContext Ctx = new(
        AppointmentId: 7,
        PatientUserId: "patient-1",
        PatientName: "Mona Ali",
        PatientEmail: "mona@example.test",
        PatientPhone: "+201000000000",
        DoctorUserId: "doctor-1",
        DoctorName: "Dr. Ahmed Hassan",
        Date: new DateOnly(2026, 12, 1),
        Time: new TimeOnly(10, 30),
        ClinicAddress: "12 Tahrir St, Cairo");

    public static TheoryData<NotificationType, string> Recipients => new()
    {
        { NotificationType.AppointmentBooked, "doctor-1" },
        { NotificationType.AppointmentRescheduled, "doctor-1" },
        { NotificationType.AppointmentCancelled, "doctor-1" },
        { NotificationType.AppointmentConfirmed, "patient-1" },
        { NotificationType.AppointmentRejected, "patient-1" },
        { NotificationType.AppointmentCompleted, "patient-1" },
        { NotificationType.AppointmentReminder, "patient-1" },
    };

    [Theory]
    [MemberData(nameof(Recipients))]
    public void Create_returns_matching_type_and_recipient(NotificationType type, string expectedRecipient)
    {
        var notification = NotificationFactory.Create(type);

        Assert.Equal(type, notification.Type);
        Assert.Equal(expectedRecipient, notification.RecipientUserId(Ctx));
        Assert.False(string.IsNullOrWhiteSpace(notification.Title(Ctx)));
        Assert.StartsWith("/", notification.Link); // local link only
    }

    [Fact]
    public void Every_enum_value_has_a_notification()
    {
        foreach (var type in Enum.GetValues<NotificationType>())
            Assert.NotNull(NotificationFactory.Create(type));
    }

    [Fact]
    public void Unknown_type_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NotificationFactory.Create((NotificationType)999));
    }

    [Fact]
    public void Booked_message_names_patient_and_time()
    {
        var message = NotificationFactory.Create(NotificationType.AppointmentBooked).Message(Ctx);

        Assert.Contains("Mona Ali", message);
        Assert.Contains("10:30", message);
    }

    [Fact]
    public void Reminder_uses_every_channel_and_includes_clinic()
    {
        var reminder = NotificationFactory.Create(NotificationType.AppointmentReminder);

        Assert.True(reminder.SendEmail);
        Assert.True(reminder.SendSms);
        Assert.Contains("12 Tahrir St", reminder.Message(Ctx));
    }

    [Fact]
    public void Doctor_notifications_are_in_app_only()
    {
        foreach (var type in new[] { NotificationType.AppointmentBooked, NotificationType.AppointmentRescheduled, NotificationType.AppointmentCancelled })
        {
            var n = NotificationFactory.Create(type);
            Assert.False(n.SendEmail);
            Assert.False(n.SendSms);
        }
    }
}

public class ReminderRulesTests
{
    private static readonly DateTime Now = new(2026, 12, 1, 9, 0, 0);

    [Theory]
    [InlineData(2026, 12, 1, 9, 30, true)]    // in 30 minutes
    [InlineData(2026, 12, 2, 9, 0, true)]     // exactly 24 hours
    [InlineData(2026, 12, 2, 9, 30, false)]   // more than 24 hours away
    [InlineData(2026, 12, 1, 9, 0, false)]    // starting now
    [InlineData(2026, 11, 30, 10, 0, false)]  // already passed
    public void IsDue_only_within_the_next_24_hours(int y, int m, int d, int hour, int minute, bool expected)
    {
        Assert.Equal(expected, ReminderRules.IsDue(new DateOnly(y, m, d), new TimeOnly(hour, minute), Now));
    }
}
