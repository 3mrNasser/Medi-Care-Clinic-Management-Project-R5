using MediCare.Services.Interfaces;
using System.Security.Claims;

namespace MediCare.Web.Security;

public interface ICurrentUser
{
    string? UserId { get; }
    Task<int?> GetDoctorIdAsync();
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IDoctorService _doctorService;

    public CurrentUser(IHttpContextAccessor accessor, IDoctorService doctorService)
    {
        _accessor = accessor;
        _doctorService = doctorService;
    }

    public string? UserId =>
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<int?> GetDoctorIdAsync()
    {
        var userId = UserId;
        if (userId is null)
            return null;

        return await _doctorService.GetDoctorIdByUserIdAsync(userId);
    }
}