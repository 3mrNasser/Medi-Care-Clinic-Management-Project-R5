using MediCare.Data.Entities;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MediCare.Services.Implementations;

public class AuthService : IAuthService
{
    // Only these two roles can be created from the signup form. Never Admin.
    private static readonly string[] AllowedRoles = { "Patient", "Doctor" };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResult> SignupAsync(string? role, string fullName, string email, string password)
    {
        // 1. The role comes from the browser, so check it against a whitelist.
        var roleName = AllowedRoles.FirstOrDefault(r =>
            string.Equals(r, role?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (roleName == null)
            return AuthResult.Fail("", "Please choose a valid account type (Patient or Doctor).");

        email = email.Trim();
        fullName = fullName.Trim();

        // 2. Email must be unique.
        if (await _userManager.FindByEmailAsync(email) != null)
            return AuthResult.Fail("Email", "An account with this email already exists.");

        // 3. Create the user (UserName = Email).
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName };
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return FromIdentity(createResult);

        // 4. Give the role. If it fails, remove the user so nothing is half-created.
        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return FromIdentity(roleResult);
        }

        // 5. A doctor also gets an empty profile, waiting for admin approval.
        if (roleName == "Doctor")
        {
            var profile = new DoctorProfile
            {
                UserId = user.Id,
                Specialty = "",
                ClinicAddress = "",
                ConsultationFee = 0,
                IsApproved = false
            };

            try
            {
                await _unitOfWork.Doctors.AddAsync(profile);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                _unitOfWork.Doctors.Remove(profile); // stop EF from retrying the failed insert
                await _userManager.DeleteAsync(user);
                return AuthResult.Fail("", "Could not create the account. Please try again.");
            }
        }

        return AuthResult.Success();
    }

    private static AuthResult FromIdentity(IdentityResult result)
    {
        var authResult = new AuthResult();
        foreach (var error in result.Errors)
        {
            var key = error.Code.StartsWith("Password") ? "Password"
                    : error.Code.Contains("Email") || error.Code.Contains("UserName") ? "Email"
                    : "";
            authResult.Errors.Add(new AuthError(key, error.Description));
        }
        return authResult;
    }
}