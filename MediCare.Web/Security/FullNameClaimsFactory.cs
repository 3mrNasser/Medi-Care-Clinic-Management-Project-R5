using System.Security.Claims;
using MediCare.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MediCare.Web.Security;

// Adds the user's FullName to the login cookie, so views can show "Sara Ahmed" instead of the email:
//   User.FindFirst(FullNameClaimsFactory.FullNameClaim)?.Value ?? User.Identity.Name
public class FullNameClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public const string FullNameClaim = "FullName";

    public FullNameClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.FullName))
            identity.AddClaim(new Claim(FullNameClaim, user.FullName));
        return identity;
    }
}
