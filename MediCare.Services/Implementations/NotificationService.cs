using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using MediCare.Services.Notifications;
using Microsoft.Extensions.Logging;

namespace MediCare.Services.Implementations;

/// <summary>Implements <see cref="INotificationService"/>; see the interface for what each member does.</summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _realtime;
    private readonly IEmailService _email;
    private readonly ISmsSender _sms;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IRealtimeNotifier realtime,
        IEmailService email,
        ISmsSender sms,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _realtime = realtime;
        _email = email;
        _sms = sms;
        _logger = logger;
    }

    public async Task NotifyAsync(int appointmentId, NotificationType type)
    {
        try
        {
            var context = await LoadContextAsync(appointmentId);
            if (context is null)
                return;

            await DeliverAsync(NotificationFactory.Create(type), context);
        }
        catch (Exception ex)
        {
            // The booking / confirmation already succeeded; a notification problem must not undo it.
            _logger.LogError(ex, "Notification {Type} for appointment {AppointmentId} failed.", type, appointmentId);
        }
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, int take = 50)
    {
        var items = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId);
        return items.OrderByDescending(n => n.CreatedAtUtc).ThenByDescending(n => n.Id)
                    .Take(take).Select(ToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var unread = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
        return unread.Count();
    }

    public async Task<string?> MarkReadAsync(string userId, int notificationId)
    {
        var found = (await _unitOfWork.Notifications.FindAsync(n => n.Id == notificationId && n.UserId == userId)).FirstOrDefault();
        if (found is null)
            return null;

        if (!found.IsRead)
        {
            found.IsRead = true;
            _unitOfWork.Notifications.Update(found);
            await _unitOfWork.SaveChangesAsync();
        }
        return found.Link;
    }

    public async Task MarkAllReadAsync(string userId)
    {
        var unread = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
        foreach (var n in unread)
        {
            n.IsRead = true;
            _unitOfWork.Notifications.Update(n);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> SendDueRemindersAsync(DateTime now)
    {
        // Confirmed, not reminded yet, and starting within the next 24 hours.
        var today = DateOnly.FromDateTime(now);
        var lastDay = DateOnly.FromDateTime(now.Add(ReminderRules.Window));
        var candidates = await _unitOfWork.Appointments.FindAsync(a =>
            a.Status == AppointmentStatus.Confirmed
            && a.ReminderSentAtUtc == null
            && a.AppointmentDate >= today && a.AppointmentDate <= lastDay);

        var sent = 0;
        var reminder = NotificationFactory.Create(NotificationType.AppointmentReminder);
        foreach (var appointment in candidates.Where(a => ReminderRules.IsDue(a.AppointmentDate, a.StartTime, now)))
        {
            try
            {
                // Mark first, so a crash halfway never sends the same reminder twice.
                appointment.ReminderSentAtUtc = DateTime.UtcNow;
                _unitOfWork.Appointments.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                var context = await LoadContextAsync(appointment.Id);
                if (context is null)
                    continue;

                await DeliverAsync(reminder, context);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reminder for appointment {AppointmentId} failed.", appointment.Id);
            }
        }
        return sent;
    }

    // ----- helpers -----

    private async Task DeliverAsync(IAppointmentNotification kind, AppointmentNotificationContext context)
    {
        var recipient = kind.RecipientUserId(context);
        var notification = new Notification
        {
            UserId = recipient,
            Type = kind.Type,
            Title = kind.Title(context),
            Message = kind.Message(context),
            Link = kind.Link,
            AppointmentId = context.AppointmentId,
            CreatedAtUtc = DateTime.UtcNow
        };

        // 1. In-app (saved, so it shows on the Notifications page even if the user is offline).
        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        // 2. Live push to open tabs (SignalR).
        try
        {
            var unread = await GetUnreadCountAsync(recipient);
            await _realtime.PushAsync(recipient, ToDto(notification), unread);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Live push of notification {Id} failed.", notification.Id);
        }

        // 3. Email / SMS: only patient-facing types ask for them.
        var toPatient = recipient == context.PatientUserId;
        // The patient's notification preferences (profile page) decide whether email / SMS are used.
        if (kind.SendEmail && toPatient && context.PatientWantsEmail && !string.IsNullOrWhiteSpace(context.PatientEmail))
        {
            var body = $"""
                <p>Hello {System.Net.WebUtility.HtmlEncode(context.PatientName)},</p>
                <p>{System.Net.WebUtility.HtmlEncode(notification.Message)}</p>
                <p style="color:#64748B">Vitala · MediCare</p>
                """;
            await _email.SendAsync(context.PatientEmail!, notification.Title, body);
        }

        if (kind.SendSms && toPatient && context.PatientWantsSms && !string.IsNullOrWhiteSpace(context.PatientPhone))
            await _sms.SendAsync(context.PatientPhone!, $"Vitala: {notification.Message}");
    }

    private async Task<AppointmentNotificationContext?> LoadContextAsync(int appointmentId)
    {
        var found = await _unitOfWork.Appointments.FindAsync(
            a => a.Id == appointmentId,
            a => a.Patient,
            a => a.DoctorProfile.User);
        var a = found.FirstOrDefault();
        if (a is null)
            return null;

        return new AppointmentNotificationContext(
            a.Id,
            a.PatientId,
            a.Patient.FullName,
            a.Patient.Email,
            a.Patient.PhoneNumber,
            a.DoctorProfile.UserId,
            a.DoctorProfile.User.FullName,
            a.AppointmentDate,
            a.StartTime,
            a.DoctorProfile.ClinicAddress,
            a.Patient.EmailNotificationsEnabled,
            a.Patient.SmsNotificationsEnabled);
    }

    private static NotificationDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type.ToString(),
        Title = n.Title,
        Message = n.Message,
        Link = n.Link,
        IsRead = n.IsRead,
        CreatedAtUtc = n.CreatedAtUtc
    };
}
