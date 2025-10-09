using FeaneMVC.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FeaneMVC.Infrastructure.Identity;

public class CustomUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<UserData>
{
    private readonly IOptions<IdentityOptions> _optionsAccessor;

    public CustomUserClaimsPrincipalFactory(IOptions<IdentityOptions> optionsAccessor)
    {
        _optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
    }

    public Task<ClaimsPrincipal> CreateAsync(UserData user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var identityOptions = _optionsAccessor.Value;
        var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);

        var userIdClaimType = identityOptions.ClaimsIdentity.UserIdClaimType;
        var userNameClaimType = identityOptions.ClaimsIdentity.UserNameClaimType;
        var roleClaimType = identityOptions.ClaimsIdentity.RoleClaimType;

        identity.AddClaim(new Claim(userIdClaimType, user.Id.ToString()));

        var displayName = !string.IsNullOrWhiteSpace(user.Username) ? user.Username : user.Email ?? user.Id.ToString();
        identity.AddClaim(new Claim(userNameClaimType, displayName));

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        }

        identity.AddClaim(new Claim(roleClaimType, user.Roles.ToString()));

        return Task.FromResult(new ClaimsPrincipal(identity));
    }
}
