using MediCare.Data.Entities;

namespace MediCare.Services.DTOs;

public class LoginResult
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }
    public ApplicationUser? User { get; init; }
    public string? Role { get; init; }

    // One generic message for every failure, so nobody can tell if an email exists.
    public static LoginResult Fail() => new() { Error = "Invalid email or password." };
}