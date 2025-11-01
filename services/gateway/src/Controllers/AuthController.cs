using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FeaneGateway.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwt;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserRepository _users;

    public AuthController(IUserRepository users, IJwtTokenService jwt, ILogger<AuthController> logger)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(OperationResult<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _users.RegisterAsync(request, cancellationToken);
        if (!result.Status || result.Data is null)
        {
            _logger.LogWarning("Registration failed for {Email}: {Message}", request.Email, result.Message);
            return BadRequest(result);
        }

        return Ok(new
        {
            result.Message,
            User = MapUser(result.Data)
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await _users.AuthenticateAsync(request, cancellationToken);
        if (!result.Status || result.Data is null)
        {
            _logger.LogWarning("Authentication failed for credential {Credential}: {Message}", request.Credential,
                result.Message);
            return Unauthorized(result);
        }

        var token = _jwt.GenerateToken(result.Data);
        var refreshToken = _jwt.GenerateRefreshToken(result.Data);

        return Ok(new
        {
            Token = token,
            RefreshToken = refreshToken,
            User = MapUser(result.Data)
        });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh token request received");
        
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            _logger.LogWarning("Refresh token is missing in request");
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            _logger.LogDebug("Validating refresh token...");
            var principal = _jwt.ValidateRefreshToken(request.RefreshToken);
            if (principal == null)
            {
                _logger.LogWarning("Invalid refresh token provided");
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            var userIdClaim = principal.FindFirst("nameid")?.Value ?? principal.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid token claims" });

            var user = await _users.FindByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return Unauthorized(new { message = "User not found" });
            }

            var newToken = _jwt.GenerateToken(user);
            var newRefreshToken = _jwt.GenerateRefreshToken(user);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

            return Ok(new
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                User = MapUser(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Unauthorized(new { message = "Failed to refresh token" });
        }
    }

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst("nameid")?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(idClaim, out var userId)) return NotFound();

        var user = await _users.FindByIdAsync(userId, cancellationToken);
        if (user is null) return NotFound();

        return Ok(MapUser(user));
    }

    private static object MapUser(User user)
    {
        return new
        {
            user.Id,
            user.Username,
            user.Email,
            Role = user.Role.ToString(),
            user.FirstRegisterTime,
            user.FirstLoginTime
        };
    }
}