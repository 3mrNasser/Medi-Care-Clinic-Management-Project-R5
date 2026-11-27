using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Common;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Implementations;

public class SpecializationService : ISpecializationService
{
    private readonly IUnitOfWork _unitOfWork;

    public SpecializationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<SpecializationDto>> GetAllAsync(bool includeHidden)
    {
        var items = await _unitOfWork.Specializations.FindAsync(s => includeHidden || s.IsActive);
        var doctors = await _unitOfWork.Doctors.GetAllAsync();
        var counts = doctors.GroupBy(d => d.Specialty.Trim(), StringComparer.OrdinalIgnoreCase)
                            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        return items.OrderBy(s => s.Name).Select(s => new SpecializationDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IsActive = s.IsActive,
            DoctorCount = counts.GetValueOrDefault(s.Name)
        }).ToList();
    }

    public async Task<IReadOnlyList<string>> GetActiveNamesAsync() =>
        (await _unitOfWork.Specializations.FindAsync(s => s.IsActive)).Select(s => s.Name).OrderBy(n => n).ToList();

    public async Task<ServiceResult> AddAsync(string name, string? description)
    {
        var check = await ValidateAsync(null, name, description);
        if (!check.Success) return check;

        await _unitOfWork.Specializations.AddAsync(new Specialization { Name = name.Trim(), Description = Clean(description) });
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(int id, string name, string? description, bool isActive)
    {
        var item = await _unitOfWork.Specializations.GetByIdAsync(id);
        if (item is null) return ServiceResult.Fail("Specialization not found.");

        var check = await ValidateAsync(id, name, description);
        if (!check.Success) return check;

        var oldName = item.Name;
        item.Name = name.Trim();
        item.Description = Clean(description);
        item.IsActive = isActive;
        _unitOfWork.Specializations.Update(item);

        // Renaming keeps doctors in the same specialization.
        if (!string.Equals(oldName, item.Name, StringComparison.Ordinal))
        {
            foreach (var d in await _unitOfWork.Doctors.FindAsync(d => d.Specialty == oldName))
            {
                d.Specialty = item.Name;
                _unitOfWork.Doctors.Update(d);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    private async Task<ServiceResult> ValidateAsync(int? id, string name, string? description)
    {
        name = (name ?? "").Trim();
        if (name.Length is 0 or > 100) return ServiceResult.Fail("Name is required (up to 100 characters).");
        if (description is { Length: > 300 }) return ServiceResult.Fail("Description must be 300 characters or fewer.");
        var same = await _unitOfWork.Specializations.FindAsync(s => s.Name == name && s.Id != (id ?? 0));
        return same.Any() ? ServiceResult.Fail("A specialization with this name already exists.") : ServiceResult.Ok();
    }

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

public class PatientProfileService : IPatientProfileService
{
    private static readonly string[] Genders = { "Male", "Female" };
    private static readonly string[] BloodTypes = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

    private readonly IUnitOfWork _unitOfWork;

    public PatientProfileService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PatientProfileDto?> GetAsync(string patientId)
    {
        var user = (await _unitOfWork.Users.FindAsync(u => u.Id == patientId)).FirstOrDefault();
        if (user is null) return null;
        var p = (await _unitOfWork.PatientProfiles.FindAsync(x => x.UserId == patientId)).FirstOrDefault();

        return new PatientProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled,
            SmsNotificationsEnabled = user.SmsNotificationsEnabled,
            DateOfBirth = p?.DateOfBirth,
            Gender = p?.Gender,
            BloodType = p?.BloodType,
            Address = p?.Address,
            Allergies = p?.Allergies,
            ChronicConditions = p?.ChronicConditions,
            CurrentMedications = p?.CurrentMedications,
            MedicalHistoryNotes = p?.MedicalHistoryNotes,
            EmergencyContactName = p?.EmergencyContactName,
            EmergencyContactPhone = p?.EmergencyContactPhone
        };
    }

    public async Task<ServiceResult> SaveAsync(string patientId, PatientProfileDto input)
    {
        var user = (await _unitOfWork.Users.FindAsync(u => u.Id == patientId)).FirstOrDefault();
        if (user is null) return ServiceResult.Fail("Account not found.");

        var name = (input.FullName ?? "").Trim();
        if (name.Length is 0 or > 100) return ServiceResult.Fail("Full name is required (up to 100 characters).");
        var phone = Clean(input.PhoneNumber);
        if (phone is not null && !IsPhone(phone)) return ServiceResult.Fail("Enter the phone number with the country code, e.g. +201001234567.");
        var emergencyPhone = Clean(input.EmergencyContactPhone);
        if (emergencyPhone is not null && !IsPhone(emergencyPhone)) return ServiceResult.Fail("Enter a valid emergency contact phone number.");
        if (input.DateOfBirth is { } dob && (dob > DateOnly.FromDateTime(DateTime.Today) || dob.Year < 1900))
            return ServiceResult.Fail("Enter a valid date of birth.");
        var gender = Clean(input.Gender);
        if (gender is not null && !Genders.Contains(gender)) return ServiceResult.Fail("Choose a gender from the list.");
        var blood = Clean(input.BloodType);
        if (blood is not null && !BloodTypes.Contains(blood)) return ServiceResult.Fail("Choose a blood type from the list.");
        if (Too(input.Address, 250) || Too(input.Allergies, 500) || Too(input.ChronicConditions, 500) || Too(input.CurrentMedications, 500)
            || Too(input.MedicalHistoryNotes, 2000) || Too(input.EmergencyContactName, 100))
            return ServiceResult.Fail("One of the fields is too long.");

        user.FullName = name;
        user.PhoneNumber = phone;
        user.EmailNotificationsEnabled = input.EmailNotificationsEnabled;
        user.SmsNotificationsEnabled = input.SmsNotificationsEnabled;
        _unitOfWork.Users.Update(user);

        var p = (await _unitOfWork.PatientProfiles.FindAsync(x => x.UserId == patientId)).FirstOrDefault();
        var isNew = p is null;
        p ??= new PatientProfile { UserId = patientId };
        p.DateOfBirth = input.DateOfBirth;
        p.Gender = gender;
        p.BloodType = blood;
        p.Address = Clean(input.Address);
        p.Allergies = Clean(input.Allergies);
        p.ChronicConditions = Clean(input.ChronicConditions);
        p.CurrentMedications = Clean(input.CurrentMedications);
        p.MedicalHistoryNotes = Clean(input.MedicalHistoryNotes);
        p.EmergencyContactName = Clean(input.EmergencyContactName);
        p.EmergencyContactPhone = emergencyPhone;
        p.UpdatedAtUtc = DateTime.UtcNow;
        if (isNew) await _unitOfWork.PatientProfiles.AddAsync(p); else _unitOfWork.PatientProfiles.Update(p);

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<bool> DoctorCanViewAsync(int doctorId, string patientId) =>
        (await _unitOfWork.Appointments.FindAsync(a => a.DoctorProfileId == doctorId && a.PatientId == patientId)).Any();

    public async Task<IReadOnlyList<AppointmentDto>> GetHistoryWithDoctorAsync(int doctorId, string patientId)
    {
        var items = await _unitOfWork.Appointments.FindAsync(a => a.DoctorProfileId == doctorId && a.PatientId == patientId);
        return items.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
            .Select(a => new AppointmentDto { Id = a.Id, Date = a.AppointmentDate, StartTime = a.StartTime, Status = a.Status, Reason = a.ReasonForVisit })
            .ToList();
    }

    private static bool IsPhone(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, @"^\+?[0-9 ]{8,20}$");
    private static bool Too(string? s, int max) => s is not null && s.Trim().Length > max;
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

public class MedicalDocumentService : IMedicalDocumentService
{
    public const long MaxBytes = 10 * 1024 * 1024; // 10 MB
    public static readonly string[] Categories = { "Test result", "X-ray", "Report", "Other" };

    // Allowed types: the content type is decided here from the extension AND the file's first bytes, never from the browser.
    private static readonly Dictionary<string, (string ContentType, byte[][] Magic)> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ("application/pdf", new[] { "%PDF"u8.ToArray() }),
        [".png"] = ("image/png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } }),
        [".jpg"] = ("image/jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } }),
        [".jpeg"] = ("image/jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } }),
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorage _storage;
    private readonly IPatientProfileService _profiles;

    public MedicalDocumentService(IUnitOfWork unitOfWork, IFileStorage storage, IPatientProfileService profiles)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _profiles = profiles;
    }

    public async Task<IReadOnlyList<MedicalDocumentDto>> GetForPatientAsync(string patientId, string viewerUserId)
    {
        var docs = await _unitOfWork.MedicalDocuments.FindAsync(d => d.PatientId == patientId);
        var uploaderIds = docs.Select(d => d.UploadedByUserId).Distinct().ToList();
        var names = (await _unitOfWork.Users.FindAsync(u => uploaderIds.Contains(u.Id))).ToDictionary(u => u.Id, u => u.FullName);
        return docs.OrderByDescending(d => d.UploadedAtUtc).Select(d => ToDto(d, names, viewerUserId)).ToList();
    }

    public async Task<ServiceResult> UploadAsync(string patientId, string uploaderUserId, int? uploaderDoctorId,
        string title, string category, string fileName, long size, Stream content)
    {
        if (uploaderDoctorId is { } doctorId && !await _profiles.DoctorCanViewAsync(doctorId, patientId))
            return ServiceResult.Fail("You can only add files for your own patients.");
        if (uploaderDoctorId is null && uploaderUserId != patientId)
            return ServiceResult.Fail("You can only add files to your own record.");

        title = (title ?? "").Trim();
        if (title.Length is 0 or > 150) return ServiceResult.Fail("Give the file a title (up to 150 characters).");
        if (!Categories.Contains(category)) category = "Other";
        if (size <= 0) return ServiceResult.Fail("Choose a file to upload.");
        if (size > MaxBytes) return ServiceResult.Fail("The file is larger than 10 MB.");

        var ext = Path.GetExtension(fileName ?? "");
        if (!Allowed.TryGetValue(ext, out var type))
            return ServiceResult.Fail("Only PDF, JPG and PNG files are allowed.");

        // Check the real file type (first bytes), so a renamed file can't pass as a PDF or an image.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        if (bytes.LongLength > MaxBytes) return ServiceResult.Fail("The file is larger than 10 MB.");
        if (!type.Magic.Any(m => bytes.Length >= m.Length && bytes.AsSpan(0, m.Length).SequenceEqual(m)))
            return ServiceResult.Fail("The file content doesn't match its type. Upload a real PDF, JPG or PNG.");

        buffer.Position = 0;
        var stored = await _storage.SaveAsync(buffer, ext.ToLowerInvariant());
        await _unitOfWork.MedicalDocuments.AddAsync(new MedicalDocument
        {
            PatientId = patientId,
            UploadedByUserId = uploaderUserId,
            Title = title,
            Category = category,
            OriginalFileName = Path.GetFileName(fileName ?? "file" + ext),
            StoredFileName = stored,
            ContentType = type.ContentType,
            SizeBytes = bytes.LongLength,
            UploadedAtUtc = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<(MedicalDocumentDto Document, Stream Content)?> OpenAsync(int documentId, string viewerUserId, int? viewerDoctorId)
    {
        var doc = await _unitOfWork.MedicalDocuments.GetByIdAsync(documentId);
        if (doc is null) return null;

        var allowed = doc.PatientId == viewerUserId
                      || (viewerDoctorId is { } d && await _profiles.DoctorCanViewAsync(d, doc.PatientId));
        if (!allowed) return null;

        var stream = await _storage.OpenReadAsync(doc.StoredFileName);
        return stream is null ? null : (ToDto(doc, new Dictionary<string, string>(), viewerUserId), stream);
    }

    public async Task<ServiceResult> DeleteAsync(int documentId, string userId)
    {
        var doc = await _unitOfWork.MedicalDocuments.GetByIdAsync(documentId);
        // The patient, or whoever uploaded it, can remove it.
        if (doc is null || (doc.PatientId != userId && doc.UploadedByUserId != userId))
            return ServiceResult.Fail("File not found.");

        _unitOfWork.MedicalDocuments.Remove(doc);
        await _unitOfWork.SaveChangesAsync();
        await _storage.DeleteAsync(doc.StoredFileName);
        return ServiceResult.Ok();
    }

    private static MedicalDocumentDto ToDto(MedicalDocument d, IReadOnlyDictionary<string, string> names, string viewerUserId) => new()
    {
        Id = d.Id,
        PatientId = d.PatientId,
        Title = d.Title,
        Category = d.Category,
        OriginalFileName = d.OriginalFileName,
        ContentType = d.ContentType,
        SizeBytes = d.SizeBytes,
        UploadedAtUtc = d.UploadedAtUtc,
        UploadedByName = d.UploadedByUserId == viewerUserId ? "you"
                       : d.UploadedByUserId == d.PatientId ? "the patient"
                       : names.GetValueOrDefault(d.UploadedByUserId) ?? "a doctor",
        UploadedByMe = d.UploadedByUserId == viewerUserId
    };
}

public class TimeOffService : ITimeOffService
{
    private readonly IUnitOfWork _unitOfWork;

    public TimeOffService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<TimeOffDto>> GetUpcomingAsync(int doctorId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var items = await _unitOfWork.TimeOffs.FindAsync(t => t.DoctorProfileId == doctorId && t.Date >= today);
        return items.OrderBy(t => t.Date).Select(t => new TimeOffDto { Id = t.Id, Date = t.Date, Reason = t.Reason }).ToList();
    }

    public async Task<ServiceResult> AddAsync(int doctorId, DateOnly date, string? reason)
    {
        if (date < DateOnly.FromDateTime(DateTime.Now)) return ServiceResult.Fail("Choose today or a future date.");
        if (reason is { Length: > 200 }) return ServiceResult.Fail("Reason must be 200 characters or fewer.");
        if ((await _unitOfWork.TimeOffs.FindAsync(t => t.DoctorProfileId == doctorId && t.Date == date)).Any())
            return ServiceResult.Fail("This day is already marked as off.");

        // Don't silently leave patients booked on a day off.
        var booked = await _unitOfWork.Appointments.FindAsync(a => a.DoctorProfileId == doctorId && a.AppointmentDate == date
            && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));
        if (booked.Any())
            return ServiceResult.Fail($"You have {booked.Count()} appointment(s) on that day. Reject or reschedule them first.");

        await _unitOfWork.TimeOffs.AddAsync(new DoctorTimeOff { DoctorProfileId = doctorId, Date = date, Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim() });
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveAsync(int doctorId, int timeOffId)
    {
        var item = await _unitOfWork.TimeOffs.GetByIdAsync(timeOffId);
        if (item is null || item.DoctorProfileId != doctorId) return ServiceResult.Fail("Day off not found.");
        _unitOfWork.TimeOffs.Remove(item);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
