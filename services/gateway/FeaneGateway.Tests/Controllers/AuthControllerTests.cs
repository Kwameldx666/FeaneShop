using System.Security.Claims;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Controllers;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.ValueObjects;
using FeaneGateway.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit;

namespace FeaneGateway.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _sut = new AuthController(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object
        );
    }

    private User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Role = Role.User,
            FirstRegisterTime = DateTime.UtcNow,
            FirstLoginTime = DateTime.UtcNow
        };
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithTokens()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Credential = "testuser",
            Password = "password123"
        };

        var user = CreateTestUser();
        var accessToken = "test.access.token";
        var refreshToken = "test.refresh.token";

        _userRepositoryMock
            .Setup(x => x.AuthenticateAsync(loginRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<User> { Status = true, Data = user });

        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns(accessToken);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken(user))
            .Returns(refreshToken);

        // Act
        var result = await _sut.Login(loginRequest, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseValue = okResult.Value;
        
        responseValue.Should().NotBeNull();
        responseValue.GetType().GetProperty("Token")?.GetValue(responseValue).Should().Be(accessToken);
        responseValue.GetType().GetProperty("RefreshToken")?.GetValue(responseValue).Should().Be(refreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Credential = "testuser",
            Password = "wrongpassword"
        };

        _userRepositoryMock
            .Setup(x => x.AuthenticateAsync(loginRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<User> { Status = false, Message = "Invalid credentials" });

        // Act
        var result = await _sut.Login(loginRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "valid.refresh.token"
        };

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "refresh")
        }));

        var user = CreateTestUser();
        user.Id = userId;

        var newAccessToken = "new.access.token";
        var newRefreshToken = "new.refresh.token";

        _jwtTokenServiceMock
            .Setup(x => x.ValidateRefreshToken(refreshRequest.RefreshToken))
            .Returns(claims);

        _userRepositoryMock
            .Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns(newAccessToken);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken(user))
            .Returns(newRefreshToken);

        // Act
        var result = await _sut.Refresh(refreshRequest, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseValue = okResult.Value;
        
        responseValue.Should().NotBeNull();
        responseValue.GetType().GetProperty("Token")?.GetValue(responseValue).Should().Be(newAccessToken);
        responseValue.GetType().GetProperty("RefreshToken")?.GetValue(responseValue).Should().Be(newRefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid.refresh.token"
        };

        _jwtTokenServiceMock
            .Setup(x => x.ValidateRefreshToken(refreshRequest.RefreshToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        var result = await _sut.Refresh(refreshRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithEmptyRefreshToken_ShouldReturnBadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = string.Empty
        };

        // Act
        var result = await _sut.Refresh(refreshRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Refresh_WhenUserNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "valid.refresh.token"
        };

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("token_type", "refresh")
        }));

        _jwtTokenServiceMock
            .Setup(x => x.ValidateRefreshToken(refreshRequest.RefreshToken))
            .Returns(claims);

        _userRepositoryMock
            .Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Refresh(refreshRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithMissingUserIdClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "valid.refresh.token"
        };

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("token_type", "refresh")
            // Missing NameIdentifier claim
        }));

        _jwtTokenServiceMock
            .Setup(x => x.ValidateRefreshToken(refreshRequest.RefreshToken))
            .Returns(claims);

        // Act
        var result = await _sut.Refresh(refreshRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "password123"
        };

        var user = CreateTestUser();

        _userRepositoryMock
            .Setup(x => x.RegisterAsync(registerRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<User> 
            { 
                Status = true, 
                Data = user, 
                Message = "Registration successful" 
            });

        // Act
        var result = await _sut.Register(registerRequest, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_WithExistingUser_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "password123"
        };

        _userRepositoryMock
            .Setup(x => x.RegisterAsync(registerRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<User> 
            { 
                Status = false, 
                Message = "User already exists" 
            });

        // Act
        var result = await _sut.Register(registerRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}

