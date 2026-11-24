namespace MediCare.Services.Common;

public class ServiceResult
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }

    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string error) => new() { Success = false, Error = error };
}