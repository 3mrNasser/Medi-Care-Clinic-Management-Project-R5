using MediCare.Data.Context;
using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Implementations;
using MediCare.Services.DTOs;
using MediCare.Services.Implementations;
using MediCare.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediCare.Tests;

// Integration tests: the real services, repositories and Unit of Work on an in-memory SQLite database.
public sealed class AppointmentServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly AppointmentService _appointments;
    private readonly NotificationService _notifications;
    private readonly FakeRealtime _realtime = new();
    private readonly FakeEmail _email = new();
    private readonly FakeSms _sms = new();

    private const string PatientId = "patient-1";
    private const string DoctorUserId = "doctor-user-1";
    private int _doctorId;
    private readonly DateOnly _day;

    public AppointmentServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        // A bookable day one week from now.
        _day = DateOnly.FromDateTime(DateTime.Now.AddDays(7));
        Seed();

        var uow = new UnitOfWork(_db);
        _notifications = new NotificationService(uow, _realtime, _email, _sms, NullLogger<NotificationService>.Instance);
        _appointments = new AppointmentService(uow, _notifications);
    }

    private void Seed()
    {
        _db.Users.Add(new ApplicationUser { Id = PatientId, UserName = "p@test", Email = "p@test", FullName = "Mona Ali", PhoneNumber = "+201000000000" });
        _db.Users.Add(new ApplicationUser { Id = DoctorUserId, UserName = "d@test", Email = "d@test", FullName = "Dr. Ahmed Hassan" });
        var doctor = new DoctorProfile { UserId = DoctorUserId, Specialty = "Cardiology", ClinicAddress = "12 Tahrir St", ConsultationFee = 400, IsApproved = true };
        _db.DoctorProfiles.Add(doctor);
        _db.SaveChanges();
        _doctorId = doctor.Id;
        _db.AvailabilitySlots.Add(new AvailabilitySlot { DoctorProfileId = _doctorId, DayOfWeek = _day.DayOfWeek, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(11, 0) });
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private Notification[] NotificationsFor(string userId) =>
        _db.Notifications.AsNoTracking().Where(n => n.UserId == userId).ToArray();

    [Fact]
    public async Task Booking_creates_pending_appointment_and_notifies_the_doctor()
    {
        var result = await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), "Chest pain");

        Assert.True(result.Success, result.Error);
        var appointment = Assert.Single(_db.Appointments.AsNoTracking());
        Assert.Equal(AppointmentStatus.Pending, appointment.Status);

        var note = Assert.Single(NotificationsFor(DoctorUserId));
        Assert.Equal(NotificationType.AppointmentBooked, note.Type);
        Assert.Contains("Mona Ali", note.Message);
        Assert.Single(_realtime.Pushed, p => p.UserId == DoctorUserId);
    }

    [Fact]
    public async Task Follow_up_without_an_earlier_completed_visit_is_refused()
    {
        var result = await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null, AppointmentType.FollowUp);

        Assert.False(result.Success);
        Assert.Contains("follow-up", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(_db.Appointments.AsNoTracking());
    }

    [Fact]
    public async Task Follow_up_after_a_recent_completed_visit_is_booked_with_its_type()
    {
        var slotId = _db.AvailabilitySlots.AsNoTracking().Single().Id;
        _db.Appointments.Add(new Appointment
        {
            PatientId = PatientId, DoctorProfileId = _doctorId, AvailabilitySlotId = slotId,
            AppointmentDate = _day.AddDays(-14), StartTime = new TimeOnly(9, 0), Status = AppointmentStatus.Completed
        });
        _db.SaveChanges();

        var result = await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null, AppointmentType.FollowUp);

        Assert.True(result.Success, result.Error);
        var booked = _db.Appointments.AsNoTracking().Single(a => a.Status == AppointmentStatus.Pending);
        Assert.Equal(AppointmentType.FollowUp, booked.Type);
        var dto = Assert.Single(await _appointments.GetPatientAppointmentsAsync(PatientId), a => a.Status == AppointmentStatus.Pending);
        Assert.Equal("Follow-up", dto.TypeName);
    }

    [Fact]
    public async Task Same_time_cannot_be_booked_twice()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 30), null);

        var second = await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 30), null);

        Assert.False(second.Success);
        Assert.Single(_db.Appointments.AsNoTracking());
    }

    [Fact]
    public async Task Time_outside_working_hours_is_rejected()
    {
        var result = await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(15, 0), null);

        Assert.False(result.Success);
        Assert.Empty(_db.Appointments.AsNoTracking());
    }

    [Fact]
    public async Task Booked_time_disappears_from_available_times()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(10, 0), null);

        var times = await _appointments.GetAvailableTimesAsync(_doctorId, _day);

        Assert.DoesNotContain(new TimeOnly(10, 0), times);
        Assert.Contains(new TimeOnly(9, 0), times);
    }

    [Fact]
    public async Task Confirming_notifies_the_patient_in_app_and_by_email()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        var id = _db.Appointments.AsNoTracking().Single().Id;

        var result = await _appointments.ConfirmAsync(_doctorId, id);

        Assert.True(result.Success, result.Error);
        var note = Assert.Single(NotificationsFor(PatientId));
        Assert.Equal(NotificationType.AppointmentConfirmed, note.Type);
        Assert.Single(_email.Sent, e => e.To == "p@test");
        Assert.Empty(_sms.Sent); // SMS is for reminders only
    }

    [Fact]
    public async Task Another_doctor_cannot_confirm()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        var id = _db.Appointments.AsNoTracking().Single().Id;

        var result = await _appointments.ConfirmAsync(_doctorId + 1, id);

        Assert.False(result.Success);
        Assert.Empty(NotificationsFor(PatientId));
    }

    [Fact]
    public async Task Cancelling_frees_the_time_and_notifies_the_doctor()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        var id = _db.Appointments.AsNoTracking().Single().Id;

        var result = await _appointments.CancelAsync(PatientId, id);

        Assert.True(result.Success, result.Error);
        Assert.Contains(new TimeOnly(9, 0), await _appointments.GetAvailableTimesAsync(_doctorId, _day));
        Assert.Contains(NotificationsFor(DoctorUserId), n => n.Type == NotificationType.AppointmentCancelled);
    }

    [Fact]
    public async Task Reminder_is_sent_once_by_every_channel()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        var id = _db.Appointments.AsNoTracking().Single().Id;
        await _appointments.ConfirmAsync(_doctorId, id);
        var dayBefore = _day.ToDateTime(new TimeOnly(9, 0)).AddHours(-20);

        var first = await _notifications.SendDueRemindersAsync(dayBefore);
        var second = await _notifications.SendDueRemindersAsync(dayBefore.AddMinutes(10));

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Single(NotificationsFor(PatientId), n => n.Type == NotificationType.AppointmentReminder);
        Assert.Single(_sms.Sent);
        Assert.NotNull(_db.Appointments.AsNoTracking().Single().ReminderSentAtUtc);
    }

    [Fact]
    public async Task No_reminder_for_pending_or_far_appointments()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null); // still pending

        var sent = await _notifications.SendDueRemindersAsync(_day.ToDateTime(new TimeOnly(9, 0)).AddHours(-2));

        Assert.Equal(0, sent);
    }

    [Fact]
    public async Task Mark_all_read_clears_the_unread_count()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 30), null);
        Assert.Equal(2, await _notifications.GetUnreadCountAsync(DoctorUserId));

        await _notifications.MarkAllReadAsync(DoctorUserId);

        Assert.Equal(0, await _notifications.GetUnreadCountAsync(DoctorUserId));
    }

    [Fact]
    public async Task A_user_cannot_open_someone_elses_notification()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        var doctorNote = NotificationsFor(DoctorUserId).Single();

        var link = await _notifications.MarkReadAsync(PatientId, doctorNote.Id);

        Assert.Null(link);
        Assert.False(_db.Notifications.AsNoTracking().Single(n => n.Id == doctorNote.Id).IsRead);
    }

    // ----- fakes for the outside world -----

    private sealed class FakeRealtime : IRealtimeNotifier
    {
        public List<(string UserId, NotificationDto Notification)> Pushed { get; } = new();
        public Task PushAsync(string userId, NotificationDto notification, int unreadCount)
        {
            Pushed.Add((userId, notification));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmail : IEmailService
    {
        public List<(string To, string Subject)> Sent { get; } = new();
        public Task SendAsync(string to, string subject, string htmlBody)
        {
            Sent.Add((to, subject));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSms : ISmsSender
    {
        public List<(string To, string Message)> Sent { get; } = new();
        public Task SendAsync(string toPhoneNumber, string message)
        {
            Sent.Add((toPhoneNumber, message));
            return Task.CompletedTask;
        }
    }
}
