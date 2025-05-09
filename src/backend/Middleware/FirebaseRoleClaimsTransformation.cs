using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.DTOs;
using TriviumParkingApp.Backend.Services;

namespace TriviumParkingApp.Backend.Middleware;

public class FirebaseRoleClaimsTransformation : IClaimsTransformation
{
    private readonly UserManager<User> _userManager;
    private readonly IUserService _userService;

    public FirebaseRoleClaimsTransformation(
        UserManager<User> userManager,
        IUserService userService)
    {
        _userManager = userManager;
        _userService = userService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity!;
        var uid = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (uid == null)
            return principal;

        var email = identity.FindFirst(ClaimTypes.Email)?.Value;

        var identityUser = await _userManager.FindByNameAsync(uid);
        if (identityUser == null)
        {
            identityUser = new User { Email = email, FirebaseUid = uid };
            await _userManager.CreateAsync(identityUser);
        }

        var domainUser = await _userService
            .SyncFirebaseUserAsync(new UserSyncRequestDto { FirebaseUid = uid }, true);

        if (domainUser != null)
        {            
            if (!identity.HasClaim(ClaimTypes.NameIdentifier, domainUser.Id.ToString()))
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, domainUser.Id.ToString()));
        }

        var roles = await _userManager.GetRolesAsync(identityUser);
        foreach (var role in roles)
            if (!identity.HasClaim(ClaimTypes.Role, role))
                identity.AddClaim(new Claim(ClaimTypes.Role, role));

        return principal;
    }
}
