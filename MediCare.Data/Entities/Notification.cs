using MediCare.Data.Enums;

namespace MediCare.Data.Entities;

// An in-app notification for one user (shown on the Notifications page and pushed live with SignalR).
public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Local page to open (e.g. "/MyAppointments"); null = no link.
    public string? Link { get; set; }

    public int? AppointmentId { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
