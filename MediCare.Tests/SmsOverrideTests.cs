using MediCare.Web.Infrastructure;

namespace MediCare.Tests;

public class SmsOverrideTests
{
    [Fact]
    public void Without_override_sends_to_patient_unchanged()
    {
        var (to, text) = TwilioSmsSender.ApplyOverride(null, "+201001234567", "Reminder");
        Assert.Equal("+201001234567", to);
        Assert.Equal("Reminder", text);
    }

    [Fact]
    public void With_override_sends_to_fixed_number_and_names_real_recipient()
    {
        var (to, text) = TwilioSmsSender.ApplyOverride(" +201112223334 ", "+201001234567", "Reminder");
        Assert.Equal("+201112223334", to);
        Assert.Equal("[to +201001234567] Reminder", text);
    }
}
