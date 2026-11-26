namespace MediCare.Services.DTOs;

public record AuthError(string Key, string Message);

public class AuthResult
{
    public List<AuthError> Errors { get; } = new();
    public bool Succeeded => Errors.Count == 0;

    public static AuthResult Success() => new();

    public static AuthResult Fail(string key, string message)
    {
        var result = new AuthResult();
        result.Errors.Add(new AuthError(key, message));
        return result;
    }
}