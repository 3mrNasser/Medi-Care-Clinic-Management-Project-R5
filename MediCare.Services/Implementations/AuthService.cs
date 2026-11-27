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
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
            return LoginResult.Fail();

        // A locked-out account gets the same generic message.
        if (await _userManager.IsLockedOutAsync(user))
            return LoginResult.Fail();

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user); // counts failures, locks after too many
            return LoginResult.Fail();
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        return new LoginResult { Succeeded = true, User = user, Role = roles.FirstOrDefault() };
    }

    // ---------- Password recovery ----------
    // The 4-digit code is stored hashed in AspNetUserTokens (no migration needed):
    // value = "{sha256(userId:code)}|{createdUtc}|{expiresUtc}|{failedAttempts}".
    private const string CodeProvider = "Vitala";
    private const string CodeName = "PasswordResetCode";
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendDelay = TimeSpan.FromSeconds(60);
    private const int MaxCodeAttempts = 5;

    public async Task<string?> CreatePasswordResetCodeAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
            return null;

        // Don't create a new code more than once a minute (stops code spamming).
        var existing = ParseCode(await _userManager.GetAuthenticationTokenAsync(user, CodeProvider, CodeName));
        if (existing != null && DateTime.UtcNow - existing.Value.Created < ResendDelay)
            return null;

        var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");
        var now = DateTime.UtcNow;
        var value = $"{HashCode(user.Id, code)}|{now:O}|{now.Add(CodeLifetime):O}|0";
        await _userManager.SetAuthenticationTokenAsync(user, CodeProvider, CodeName, value);
        return code;
    }

    public async Task<ResetCodeResult> VerifyPasswordResetCodeAsync(string email, string code)
    {
        const string invalid = "The code is wrong or has expired. Request a new code.";
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
            return ResetCodeResult.Fail(invalid);

        var stored = ParseCode(await _userManager.GetAuthenticationTokenAsync(user, CodeProvider, CodeName));
        if (stored == null || DateTime.UtcNow > stored.Value.Expires || stored.Value.Attempts >= MaxCodeAttempts)
            return ResetCodeResult.Fail(invalid);

        var given = HashCode(user.Id, (code ?? "").Trim());
        var matches = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.ASCII.GetBytes(given), System.Text.Encoding.ASCII.GetBytes(stored.Value.Hash));

        if (!matches)
        {
            var attempts = stored.Value.Attempts + 1;
            await _userManager.SetAuthenticationTokenAsync(user, CodeProvider, CodeName,
                $"{stored.Value.Hash}|{stored.Value.Created:O}|{stored.Value.Expires:O}|{attempts}");
            return ResetCodeResult.Fail(attempts >= MaxCodeAttempts
                ? "Too many wrong tries. Request a new code."
                : "The code is wrong. Check your email and try again.");
        }

        // One use only.
        await _userManager.RemoveAuthenticationTokenAsync(user, CodeProvider, CodeName);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return ResetCodeResult.Success(token);
    }

    public async Task<AuthResult> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
            return AuthResult.Fail("", "The reset link has expired. Start again.");

        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code == "InvalidToken"))
                return AuthResult.Fail("", "The reset link has expired. Start again.");
            var mapped = FromIdentity(result);
            // The reset form names the field NewPassword.
            var fixedResult = new AuthResult();
            foreach (var e in mapped.Errors)
                fixedResult.Errors.Add(e with { Key = e.Key == "Password" ? "NewPassword" : e.Key });
            return fixedResult;
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        return AuthResult.Success();
    }

    private static string HashCode(string userId, string code)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{userId}:{code}"));
        return Convert.ToHexString(bytes);
    }

    private static (string Hash, DateTime Created, DateTime Expires, int Attempts)? ParseCode(string? value)
    {
        var parts = value?.Split('|');
        if (parts is not { Length: 4 }) return null;
        if (!DateTime.TryParse(parts[1], null, System.Globalization.DateTimeStyles.RoundtripKind, out var created)) return null;
        if (!DateTime.TryParse(parts[2], null, System.Globalization.DateTimeStyles.RoundtripKind, out var expires)) return null;
        if (!int.TryParse(parts[3], out var attempts)) return null;
        return (parts[0], created, expires, attempts);
    }

    // ---------- Google / external sign-in ----------
    public async Task<LoginResult> FindOrCreateExternalUserAsync(string email, string? fullName)
    {
        email = email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // New Google users become Patients (doctors sign up with the form so they get a profile to approve).
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName.Trim()
            };
            var create = await _userManager.CreateAsync(user); // no password: signs in with Google only
            if (!create.Succeeded)
                return LoginResult.Fail();
            var role = await _userManager.AddToRoleAsync(user, "Patient");
            if (!role.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return LoginResult.Fail();
            }
        }
        else if (await _userManager.IsLockedOutAsync(user))
        {
            return LoginResult.Fail();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new LoginResult { Succeeded = true, User = user, Role = roles.FirstOrDefault() };
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