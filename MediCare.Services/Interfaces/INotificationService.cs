using MediCare.Data.Enums;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface INotificationService
{
    // Creates the notification for an appointment event (via NotificationFactory), saves it, pushes it live,
    // and sends email / SMS when that type asks for it. Never throws: a failed notification must not break booking.
    Task NotifyAsync(int appointmentId, NotificationType type);

    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, int take = 50);
    Task<int> GetUnreadCountAsync(string userId);
    // Marks one notification read and returns its link (null if not found / not the user's).
    Task<string?> MarkReadAsync(string userId, int notificationId);
    Task MarkAllReadAsync(string userId);

    // Sends the 24-hour reminder for every confirmed appointment that is due and wasn't reminded yet. Returns how many.
    Task<int> SendDueRemindersAsync(DateTime now);
}

// Pushes a saved notification to the user's open browser tabs (implemented with SignalR in MediCare.Web).
public interface IRealtimeNotifier
{
    Task PushAsync(string userId, NotificationDto notification, int unreadCount);
}

// Sends a text message (implemented with Twilio in MediCare.Web).
public interface ISmsSender
{
    Task SendAsync(string toPhoneNumber, string message);
}
