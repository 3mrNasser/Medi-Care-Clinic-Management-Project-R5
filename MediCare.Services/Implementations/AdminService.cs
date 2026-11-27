using MediCare.Data.Entities;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<DoctorAdminDto>> GetDoctorsAsync()
    {
        // d => true means "all doctors"; the second argument loads the User with each profile.
        var doctors = await _unitOfWork.Doctors.FindAsync(d => true, d => d.User);

        return doctors
            .OrderBy(d => d.User.FullName)
            .Select(d => new DoctorAdminDto(d.Id, d.User.FullName, d.User.Email ?? "", d.Specialty, d.IsApproved))
            .ToList();
    }

    public async Task<bool> SetDoctorApprovalAsync(int doctorProfileId, bool isApproved)
    {
        var profile = await _unitOfWork.Doctors.GetByIdAsync(doctorProfileId);
        if (profile == null)
            return false;

        profile.IsApproved = isApproved;
        _unitOfWork.Doctors.Update(profile);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<UserAdminDto>> GetUsersAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

        // One query per user for roles: fine for a small project.
        var result = new List<UserAdminDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserAdminDto(
                user.Id,
                user.FullName,
                user.Email ?? "",
                roles.Count == 0 ? "(none)" : string.Join(", ", roles)));
        }
        return result;
    }
}