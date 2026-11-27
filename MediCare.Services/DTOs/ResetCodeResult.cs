namespace MediCare.Services.DTOs;

public class ResetCodeResult
{
    public bool Succeeded { get; set; }
    public string? ResetToken { get; set; }
    public string? Error { get; set; }

    public static ResetCodeResult Success(string token) => new() { Succeeded = true, ResetToken = token };
    public static ResetCodeResult Fail(string error) => new() { Succeeded = false, Error = error };
}
