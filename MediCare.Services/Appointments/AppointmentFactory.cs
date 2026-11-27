using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Services.Common;

namespace MediCare.Services.Appointments;

/// <summary>What a visit type needs to check before it can be booked.</summary>
/// <param name="PatientId">The patient booking.</param>
/// <param name="DoctorId">The doctor being booked.</param>
/// <param name="Date">The requested date.</param>
/// <param name="PastVisitsWithDoctor">The patient's earlier appointments with this doctor.</param>
public record AppointmentBookingContext(string PatientId, int DoctorId, DateOnly Date, IReadOnlyList<Appointment> PastVisitsWithDoctor);

/// <summary>
/// One kind of appointment (Factory Pattern): its name, description, price and booking rule.
/// New kinds are added as new classes and one line in <see cref="AppointmentFactory"/>.
/// </summary>
public interface IAppointmentKind
{
    /// <summary>The visit type this class represents.</summary>
    AppointmentType Type { get; }

    /// <summary>Short name shown to patients and doctors.</summary>
    string DisplayName { get; }

    /// <summary>One-line explanation shown on the booking page.</summary>
    string Description { get; }

    /// <summary>Share of the doctor's consultation fee charged for this type (1.0 = full fee).</summary>
    decimal FeeMultiplier { get; }

    /// <summary>Checks the type's own rule (for example, a follow-up needs an earlier completed visit).</summary>
    ServiceResult Validate(AppointmentBookingContext context);
}

/// <summary>A normal examination. Always allowed; full fee.</summary>
public sealed class FirstVisitAppointment : IAppointmentKind
{
    public AppointmentType Type => AppointmentType.FirstVisit;
    public string DisplayName => "Examination";
    public string Description => "A full examination with the doctor.";
    public decimal FeeMultiplier => 1.0m;
    public ServiceResult Validate(AppointmentBookingContext context) => ServiceResult.Ok();
}

/// <summary>A follow-up: only after a completed visit with the same doctor in the last 60 days; half the fee.</summary>
public sealed class FollowUpAppointment : IAppointmentKind
{
    /// <summary>How long after a completed visit a follow-up can be booked.</summary>
    public const int WindowDays = 60;

    public AppointmentType Type => AppointmentType.FollowUp;
    public string DisplayName => "Follow-up";
    public string Description => $"A check after a visit with this doctor in the last {WindowDays} days (half price).";
    public decimal FeeMultiplier => 0.5m;

    public ServiceResult Validate(AppointmentBookingContext context)
    {
        var hasRecentVisit = context.PastVisitsWithDoctor.Any(a =>
            a.Status == AppointmentStatus.Completed
            && a.AppointmentDate <= context.Date
            && a.AppointmentDate >= context.Date.AddDays(-WindowDays));
        return hasRecentVisit
            ? ServiceResult.Ok()
            : ServiceResult.Fail($"A follow-up needs a completed visit with this doctor in the last {WindowDays} days. Book an examination instead.");
    }
}

/// <summary>A short consultation or second opinion; 75% of the fee.</summary>
public sealed class ConsultationAppointment : IAppointmentKind
{
    public AppointmentType Type => AppointmentType.Consultation;
    public string DisplayName => "Consultation";
    public string Description => "A short talk or second opinion, without an examination.";
    public decimal FeeMultiplier => 0.75m;
    public ServiceResult Validate(AppointmentBookingContext context) => ServiceResult.Ok();
}

/// <summary>
/// Factory Pattern for appointments: returns the right <see cref="IAppointmentKind"/> for a type,
/// and builds new <see cref="Appointment"/> entities so every booking path creates them the same way.
/// </summary>
public static class AppointmentFactory
{
    /// <summary>Every bookable kind, in the order shown on the booking page.</summary>
    public static IReadOnlyList<IAppointmentKind> All { get; } = new IAppointmentKind[]
    {
        new FirstVisitAppointment(), new FollowUpAppointment(), new ConsultationAppointment()
    };

    /// <summary>Returns the kind object for <paramref name="type"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The type is unknown.</exception>
    public static IAppointmentKind Create(AppointmentType type) => type switch
    {
        AppointmentType.FirstVisit => All[0],
        AppointmentType.FollowUp => All[1],
        AppointmentType.Consultation => All[2],
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown appointment type.")
    };

    /// <summary>Builds a new <c>Pending</c> appointment of the given kind.</summary>
    public static Appointment Build(IAppointmentKind kind, string patientId, int doctorId, int slotId,
        DateOnly date, TimeOnly time, string? reason) => new()
    {
        PatientId = patientId,
        DoctorProfileId = doctorId,
        AvailabilitySlotId = slotId,
        AppointmentDate = date,
        StartTime = time,
        Status = AppointmentStatus.Pending,
        Type = kind.Type,
        ReasonForVisit = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
    };

    /// <summary>The price of one visit of <paramref name="type"/> for a doctor whose full fee is <paramref name="consultationFee"/>.</summary>
    public static decimal PriceFor(AppointmentType type, decimal consultationFee) =>
        Math.Round(consultationFee * Create(type).FeeMultiplier, 2);
}
