namespace MediCare.Services.Common;

/// <summary>The outcome of a service action: success, or a message to show the user.</summary>
public class ServiceResult
{
    /// <summary>True when the action succeeded.</summary>
    public bool Success { get; private set; }

    /// <summary>A user-facing reason when <see cref="Success"/> is false.</summary>
    public string? Error { get; private set; }

    /// <summary>A successful result.</summary>
    public static ServiceResult Ok() => new() { Success = true };

    /// <summary>A failed result with a user-facing message.</summary>
    public static ServiceResult Fail(string error) => new() { Success = false, Error = error };
}
