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

// Integration tests for days off, medical documents, specializations, the patient profile and notification preferences.
public sealed class ClinicServicesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly UnitOfWork _uow;
    private readonly AppointmentService _appointments;
    private readonly TimeOffService _timeOff;
    private readonly PatientProfileService _profiles;
    private readonly MedicalDocumentService _documents;
    private readonly SpecializationService _specializations;
    private readonly MemoryStorage _storage = new();
    private readonly CountingEmail _email = new();

    private const string PatientId = "patient-1";
    private const string OtherPatientId = "patient-2";
    private const string DoctorUserId = "doctor-user-1";
    private const string OtherDoctorUserId = "doctor-user-2";
    private int _doctorId, _otherDoctorId;
    private readonly DateOnly _day = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

    public ClinicServicesTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();

        _db.Users.AddRange(
            new ApplicationUser { Id = PatientId, UserName = "p@test", Email = "p@test", FullName = "Mona Ali" },
            new ApplicationUser { Id = OtherPatientId, UserName = "p2@test", Email = "p2@test", FullName = "Omar Nabil" },
            new ApplicationUser { Id = DoctorUserId, UserName = "d@test", Email = "d@test", FullName = "Dr. Ahmed Hassan" },
            new ApplicationUser { Id = OtherDoctorUserId, UserName = "d2@test", Email = "d2@test", FullName = "Dr. Sara Mostafa" });
        var doctor = new DoctorProfile { UserId = DoctorUserId, Specialty = "Cardiology", ClinicAddress = "Cairo", ConsultationFee = 400, IsApproved = true };
        var other = new DoctorProfile { UserId = OtherDoctorUserId, Specialty = "Dermatology", ClinicAddress = "Giza", ConsultationFee = 300, IsApproved = true };
        _db.DoctorProfiles.AddRange(doctor, other);
        _db.Specializations.AddRange(new Specialization { Name = "Cardiology" }, new Specialization { Name = "Dermatology" });
        _db.SaveChanges();
        _doctorId = doctor.Id;
        _otherDoctorId = other.Id;
        _db.AvailabilitySlots.Add(new AvailabilitySlot { DoctorProfileId = _doctorId, DayOfWeek = _day.DayOfWeek, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(11, 0) });
        _db.SaveChanges();

        _uow = new UnitOfWork(_db);
        var notifications = new NotificationService(_uow, new NoRealtime(), _email, new NoSms(), NullLogger<NotificationService>.Instance);
        _appointments = new AppointmentService(_uow, notifications);
        _timeOff = new TimeOffService(_uow);
        _profiles = new PatientProfileService(_uow);
        _documents = new MedicalDocumentService(_uow, _storage, _profiles);
        _specializations = new SpecializationService(_uow);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ----- Days off -----

    [Fact]
    public async Task Day_off_removes_all_free_times()
    {
        Assert.NotEmpty(await _appointments.GetAvailableTimesAsync(_doctorId, _day));

        var result = await _timeOff.AddAsync(_doctorId, _day, "Conference");

        Assert.True(result.Success, result.Error);
        Assert.Empty(await _appointments.GetAvailableTimesAsync(_doctorId, _day));
        Assert.False((await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null)).Success);
    }

    [Fact]
    public async Task Day_off_is_refused_when_patients_are_booked()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);

        var result = await _timeOff.AddAsync(_doctorId, _day, null);

        Assert.False(result.Success);
        Assert.Contains("1 appointment", result.Error);
    }

    [Fact]
    public async Task Day_off_in_the_past_is_refused()
    {
        var result = await _timeOff.AddAsync(_doctorId, DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), null);
        Assert.False(result.Success);
    }

    // ----- Medical documents -----

    private static MemoryStream Png() => new(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3 });

    [Fact]
    public async Task Patient_uploads_a_real_png()
    {
        var result = await _documents.UploadAsync(PatientId, PatientId, null, "Chest X-ray", "X-ray", "xray.png", 11, Png());

        Assert.True(result.Success, result.Error);
        var doc = Assert.Single(await _documents.GetForPatientAsync(PatientId, PatientId));
        Assert.Equal("image/png", doc.ContentType);
        Assert.Single(_storage.Files);
    }

    [Fact]
    public async Task Renamed_file_with_wrong_content_is_rejected()
    {
        var fakePdf = new MemoryStream("<html>not a pdf</html>"u8.ToArray());

        var result = await _documents.UploadAsync(PatientId, PatientId, null, "Report", "Report", "report.pdf", fakePdf.Length, fakePdf);

        Assert.False(result.Success);
        Assert.Empty(_storage.Files);
    }

    [Theory]
    [InlineData("virus.exe")]
    [InlineData("page.html")]
    [InlineData("noextension")]
    public async Task Other_file_types_are_rejected(string fileName)
    {
        var result = await _documents.UploadAsync(PatientId, PatientId, null, "File", "Other", fileName, 11, Png());
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Files_over_10_MB_are_rejected()
    {
        var result = await _documents.UploadAsync(PatientId, PatientId, null, "Big", "Other", "big.png", MedicalDocumentService.MaxBytes + 1, Png());
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Only_the_patient_and_their_doctors_can_open_a_file()
    {
        await _documents.UploadAsync(PatientId, PatientId, null, "Blood test", "Test result", "blood.png", 11, Png());
        var id = (await _documents.GetForPatientAsync(PatientId, PatientId)).Single().Id;
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null); // patient booked with doctor 1

        Assert.NotNull(await _documents.OpenAsync(id, PatientId, null));                 // the patient
        Assert.NotNull(await _documents.OpenAsync(id, DoctorUserId, _doctorId));         // their doctor
        Assert.Null(await _documents.OpenAsync(id, OtherDoctorUserId, _otherDoctorId));  // another doctor
        Assert.Null(await _documents.OpenAsync(id, OtherPatientId, null));               // another patient
    }

    [Fact]
    public async Task Doctor_can_upload_only_for_own_patients()
    {
        var notMine = await _documents.UploadAsync(PatientId, DoctorUserId, _doctorId, "Report", "Report", "r.png", 11, Png());
        Assert.False(notMine.Success);

        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        var mine = await _documents.UploadAsync(PatientId, DoctorUserId, _doctorId, "Report", "Report", "r.png", 11, Png());
        Assert.True(mine.Success, mine.Error);
    }

    [Fact]
    public async Task Another_patient_cannot_delete_a_file()
    {
        await _documents.UploadAsync(PatientId, PatientId, null, "Scan", "X-ray", "s.png", 11, Png());
        var id = (await _documents.GetForPatientAsync(PatientId, PatientId)).Single().Id;

        Assert.False((await _documents.DeleteAsync(id, OtherPatientId)).Success);
        Assert.True((await _documents.DeleteAsync(id, PatientId)).Success);
        Assert.Empty(_storage.Files);
    }

    // ----- Specializations -----

    [Fact]
    public async Task Duplicate_specialization_is_refused()
    {
        Assert.False((await _specializations.AddAsync("Cardiology", null)).Success);
        Assert.True((await _specializations.AddAsync("Neurology", "Brain and nerves")).Success);
    }

    [Fact]
    public async Task Renaming_a_specialization_moves_its_doctors()
    {
        var cardio = (await _specializations.GetAllAsync(true)).Single(s => s.Name == "Cardiology");
        Assert.Equal(1, cardio.DoctorCount);

        await _specializations.UpdateAsync(cardio.Id, "Cardiology & Vascular", null, true);

        Assert.Equal("Cardiology & Vascular", _db.DoctorProfiles.AsNoTracking().Single(d => d.Id == _doctorId).Specialty);
    }

    [Fact]
    public async Task Hidden_specialization_is_not_offered()
    {
        var derm = (await _specializations.GetAllAsync(true)).Single(s => s.Name == "Dermatology");
        await _specializations.UpdateAsync(derm.Id, derm.Name, null, isActive: false);

        Assert.DoesNotContain("Dermatology", await _specializations.GetActiveNamesAsync());
    }

    // ----- Patient profile and notification preferences -----

    [Fact]
    public async Task Profile_is_saved_and_validated()
    {
        var bad = await _profiles.SaveAsync(PatientId, new PatientProfileDto { FullName = "Mona Ali", PhoneNumber = "call me" });
        Assert.False(bad.Success);

        var ok = await _profiles.SaveAsync(PatientId, new PatientProfileDto
        {
            FullName = "Mona Ali", PhoneNumber = "+201001234567", Gender = "Female", BloodType = "O+",
            DateOfBirth = new DateOnly(1995, 5, 20), Allergies = "Penicillin"
        });
        Assert.True(ok.Success, ok.Error);

        var saved = await _profiles.GetAsync(PatientId);
        Assert.Equal("Penicillin", saved!.Allergies);
        Assert.Equal("+201001234567", saved.PhoneNumber);
        Assert.NotNull(saved.Age);
    }

    [Fact]
    public async Task Doctor_sees_only_own_patients()
    {
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);

        Assert.True(await _profiles.DoctorCanViewAsync(_doctorId, PatientId));
        Assert.False(await _profiles.DoctorCanViewAsync(_otherDoctorId, PatientId));
    }

    [Fact]
    public async Task Email_is_skipped_when_the_patient_turned_it_off()
    {
        await _profiles.SaveAsync(PatientId, new PatientProfileDto { FullName = "Mona Ali", EmailNotificationsEnabled = false, SmsNotificationsEnabled = true });
        await _appointments.BookAsync(PatientId, _doctorId, _day, new TimeOnly(9, 0), null);
        var id = _db.Appointments.AsNoTracking().Single().Id;

        await _appointments.ConfirmAsync(_doctorId, id);

        Assert.Equal(0, _email.Count);                                                        // no email…
        Assert.Contains(_db.Notifications.AsNoTracking(), n => n.UserId == PatientId && n.Type == NotificationType.AppointmentConfirmed); // …but still in-app
    }

    // ----- fakes -----

    private sealed class MemoryStorage : IFileStorage
    {
        public Dictionary<string, byte[]> Files { get; } = new();
        public async Task<string> SaveAsync(Stream content, string extension)
        {
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            var name = Guid.NewGuid().ToString("N") + extension;
            Files[name] = ms.ToArray();
            return name;
        }
        public Task<Stream?> OpenReadAsync(string storedFileName) =>
            Task.FromResult<Stream?>(Files.TryGetValue(storedFileName, out var b) ? new MemoryStream(b) : null);
        public Task DeleteAsync(string storedFileName) { Files.Remove(storedFileName); return Task.CompletedTask; }
    }

    private sealed class CountingEmail : IEmailService
    {
        public int Count { get; private set; }
        public Task SendAsync(string to, string subject, string htmlBody) { Count++; return Task.CompletedTask; }
    }

    private sealed class NoRealtime : IRealtimeNotifier
    {
        public Task PushAsync(string userId, NotificationDto notification, int unreadCount) => Task.CompletedTask;
    }

    private sealed class NoSms : ISmsSender
    {
        public Task SendAsync(string toPhoneNumber, string message) => Task.CompletedTask;
    }
}
