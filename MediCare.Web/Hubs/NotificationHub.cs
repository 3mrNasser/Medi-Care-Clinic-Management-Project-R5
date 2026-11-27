using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MediCare.Web.Hubs;

// Real-time notifications. Each signed-in browser tab connects to /hubs/notifications; SignalR maps the connection
// to the user's Id (the NameIdentifier claim), so Clients.User(userId) reaches all of that user's tabs.
[Authorize]
public class NotificationHub : Hub
{
}

// Sends a saved notification to the user's open tabs. The page script listens for "notification".
public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotifier(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public Task PushAsync(string userId, NotificationDto notification, int unreadCount) =>
        _hub.Clients.User(userId).SendAsync("notification", new
        {
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Link,
            UnreadCount = unreadCount
        });
}
