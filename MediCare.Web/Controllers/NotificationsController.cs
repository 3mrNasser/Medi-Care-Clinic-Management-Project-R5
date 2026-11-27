using MediCare.Services.Interfaces;
using MediCare.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers;

// Notifications for the signed-in user (patients and doctors).
[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notifications;
    private readonly ICurrentUser _currentUser;

    public NotificationsController(INotificationService notifications, ICurrentUser currentUser)
    {
        _notifications = notifications;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _notifications.GetForUserAsync(_currentUser.UserId!);
        return View(items);
    }

    // Marks it read, then opens its page (local links only).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(int id)
    {
        var link = await _notifications.MarkReadAsync(_currentUser.UserId!, id);
        if (!string.IsNullOrEmpty(link) && Url.IsLocalUrl(link))
            return LocalRedirect(link);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifications.MarkAllReadAsync(_currentUser.UserId!);
        return RedirectToAction(nameof(Index));
    }

    // Unread count for the sidebar badge after the page loads (JSON).
    [HttpGet]
    public async Task<IActionResult> UnreadCount() =>
        Json(new { count = await _notifications.GetUnreadCountAsync(_currentUser.UserId!) });
}
