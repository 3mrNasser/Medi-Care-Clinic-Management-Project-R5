using MediCare.Data.Enums;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

/// <summary>In-app, real-time, email and SMS notifications about appointments.</summary>
public interface INotificationService
{
    /// <summary>
    /// Creates the notification for an appointment event (via <c>NotificationFactory</c>), saves it, pushes it live,
    /// and sends email / SMS when that type asks for it. Never throws: a failed notification must not break booking.
    /// </summary>
    Task NotifyAsync(int appointmentId, NotificationType type);

    /// <summary>The user's latest notifications, newest first.</summary>
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, int take = 50);

    /// <summary>How many of the user's notifications are unread (the bell badge).</summary>
    Task<int> GetUnreadCountAsync(string userId);

    /// <summary>Marks one notification read and returns its link (<c>null</c> if not found or not the user's).</summary>
    Task<string?> MarkReadAsync(string userId, int notificationId);

    /// <summary>Marks all of the user's notifications read.</summary>
    Task MarkAllReadAsync(string userId);

    /// <summary>Sends the 24-hour reminder for every confirmed appointment that is due and wasn't reminded yet. Returns how many.</summary>
    Task<int> SendDueRemindersAsync(DateTime now);
}

/// <summary>Pushes a saved notification to the user's open browser tabs (implemented with SignalR in MediCare.Web).</summary>
public interface IRealtimeNotifier
{
    /// <summary>Sends the notification and the new unread count to every connection of <paramref name="userId"/>.</summary>
    Task PushAsync(string userId, NotificationDto notification, int unreadCount);
}

/// <summary>Sends a text message (implemented with Twilio in MediCare.Web; logs only when Twilio isn't configured).</summary>
public interface ISmsSender
{
    /// <summary>Sends one SMS to a phone number in international format.</summary>
    Task SendAsync(string toPhoneNumber, string message);
}
