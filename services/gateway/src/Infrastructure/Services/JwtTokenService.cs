using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Configuration;
using AuthService.Domain.Entities;
using FeaneGateway.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IOptions<JwtOptions> options, ILogger<JwtTokenService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_options.IsValid()) throw new InvalidOperationException("JWT options are not configured correctly.");
    }

    public string GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("token_type", "refresh")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Refresh token expires in 7 days (configurable)
        var refreshTokenExpiration = _options.RefreshTokenExpirationDays > 0
            ? _options.RefreshTokenExpirationDays
            : 7;

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(refreshTokenExpiration),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Refresh token is null or empty");
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        try
        {
            _logger.LogDebug("Attempting to validate refresh token");
            
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            }, out _);

            // Verify it's a refresh token
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (tokenType != "refresh")
            {
                _logger.LogWarning("Token is not a refresh token. Token type: {TokenType}", tokenType);
                return null;
            }

            _logger.LogInformation("Refresh token validated successfully");
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Refresh token has expired");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return null;
        }
    }
}