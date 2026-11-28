using MediCare.Services;

namespace MediCare.Tests;

public class AuthCodeOptionsTests
{
    [Theory]
    [InlineData("1234", "1234")]
    [InlineData(" 0000 ", "0000")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("12345", null)]
    [InlineData("12a4", null)]
    public void Only_four_digit_codes_are_used(string? configured, string? expected) =>
        Assert.Equal(expected, new AuthCodeOptions { FixedCode = configured }.ValidFixedCode);
}
