using System.Security.Claims;
using AuthService.Domain.Entities;

namespace FeaneGateway.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken(User user);
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
}