using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResult> SignupAsync(string? role, string fullName, string email, string password);
    Task<LoginResult> LoginAsync(string email, string password);
}