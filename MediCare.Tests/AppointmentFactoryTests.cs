using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Services.Appointments;

namespace MediCare.Tests;

// Unit tests for the Factory Pattern used to create appointment types.
public class AppointmentFactoryTests
{
    private static readonly DateOnly Day = new(2026, 12, 10);

    private static AppointmentBookingContext Context(params Appointment[] past) =>
        new("patient-1", 1, Day, past);

    [Theory]
    [InlineData(AppointmentType.FirstVisit, typeof(FirstVisitAppointment))]
    [InlineData(AppointmentType.FollowUp, typeof(FollowUpAppointment))]
    [InlineData(AppointmentType.Consultation, typeof(ConsultationAppointment))]
    public void Create_returns_the_class_for_each_type(AppointmentType type, Type expected)
    {
        var kind = AppointmentFactory.Create(type);

        Assert.IsType(expected, kind);
        Assert.Equal(type, kind.Type);
    }

    [Fact]
    public void Every_type_has_a_kind()
    {
        foreach (var type in Enum.GetValues<AppointmentType>())
            Assert.Contains(AppointmentFactory.All, k => k.Type == type);
    }

    [Fact]
    public void Unknown_type_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AppointmentFactory.Create((AppointmentType)99));
    }

    [Theory]
    [InlineData(AppointmentType.FirstVisit, 400, 400)]
    [InlineData(AppointmentType.FollowUp, 400, 200)]
    [InlineData(AppointmentType.Consultation, 400, 300)]
    [InlineData(AppointmentType.Consultation, 333, 249.75)]
    public void PriceFor_applies_the_type_multiplier(AppointmentType type, decimal fee, decimal expected)
    {
        Assert.Equal(expected, AppointmentFactory.PriceFor(type, fee));
    }

    [Fact]
    public void Build_creates_a_pending_appointment_of_the_kind()
    {
        var a = AppointmentFactory.Build(AppointmentFactory.Create(AppointmentType.Consultation),
            "patient-1", 1, 5, Day, new TimeOnly(9, 0), "  Second opinion  ");

        Assert.Equal(AppointmentStatus.Pending, a.Status);
        Assert.Equal(AppointmentType.Consultation, a.Type);
        Assert.Equal(5, a.AvailabilitySlotId);
        Assert.Equal("Second opinion", a.ReasonForVisit);
    }

    [Fact]
    public void Follow_up_rule_needs_a_completed_visit_inside_the_window()
    {
        var followUp = AppointmentFactory.Create(AppointmentType.FollowUp);
        var recent = new Appointment { AppointmentDate = Day.AddDays(-30), Status = AppointmentStatus.Completed };
        var tooOld = new Appointment { AppointmentDate = Day.AddDays(-(FollowUpAppointment.WindowDays + 1)), Status = AppointmentStatus.Completed };
        var notCompleted = new Appointment { AppointmentDate = Day.AddDays(-5), Status = AppointmentStatus.Cancelled };

        Assert.True(followUp.Validate(Context(recent)).Success);
        Assert.False(followUp.Validate(Context(tooOld)).Success);
        Assert.False(followUp.Validate(Context(notCompleted)).Success);
        Assert.False(followUp.Validate(Context()).Success);
    }

    [Fact]
    public void First_visit_and_consultation_are_always_allowed()
    {
        Assert.True(AppointmentFactory.Create(AppointmentType.FirstVisit).Validate(Context()).Success);
        Assert.True(AppointmentFactory.Create(AppointmentType.Consultation).Validate(Context()).Success);
    }
}
