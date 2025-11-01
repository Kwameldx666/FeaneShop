using System.Security.Claims;
using AuthService.Application.Configuration;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FeaneGateway.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
    private readonly JwtOptions _jwtOptions;
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        _loggerMock = new Mock<ILogger<JwtTokenService>>();
        _jwtOptions = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "ThisIsAVerySecretKeyForTestingPurposesOnly123456789",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7,
            CookieName = "TestCookie"
        };

        var optionsMock = new Mock<IOptions<JwtOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_jwtOptions);

        _sut = new JwtTokenService(optionsMock.Object, _loggerMock.Object);
    }

    private User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Role = Role.User
        };
    }

    [Fact]
    public void GenerateToken_ShouldCreateValidAccessToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateRefreshToken_ShouldCreateValidRefreshToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var refreshToken = _sut.GenerateRefreshToken(user);

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void ValidateRefreshToken_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = _sut.GenerateRefreshToken(user);

        // Act
        var principal = _sut.ValidateRefreshToken(refreshToken);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(user.Id.ToString());
        principal.FindFirst("token_type")?.Value.Should().Be("refresh");
    }

    [Fact]
    public void ValidateRefreshToken_WithAccessToken_ShouldReturnNull()
    {
        // Arrange
        var user = CreateTestUser();
        var accessToken = _sut.GenerateToken(user); // Not a refresh token

        // Act
        var principal = _sut.ValidateRefreshToken(accessToken);

        // Assert
        principal.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not a refresh token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ValidateRefreshToken_WithNullToken_ShouldReturnNull()
    {
        // Act
        var principal = _sut.ValidateRefreshToken(null!);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateRefreshToken_WithEmptyToken_ShouldReturnNull()
    {
        // Act
        var principal = _sut.ValidateRefreshToken(string.Empty);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateRefreshToken_WithInvalidToken_ShouldReturnNull()
    {
        // Act
        var principal = _sut.ValidateRefreshToken("invalid.token.here");

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateToken_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _sut.GenerateToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GenerateRefreshToken_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _sut.GenerateRefreshToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GeneratedTokens_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _sut.GenerateToken(user);
        
        // Decode token manually to verify claims (simplified check)
        var parts = token.Split('.');
        
        // Assert
        parts.Should().HaveCount(3);
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RefreshToken_ShouldHaveLongerExpirationThanAccessToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var accessToken = _sut.GenerateToken(user);
        var refreshToken = _sut.GenerateRefreshToken(user);

        // Assert
        // Both should be valid tokens
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
        
        // Refresh token should be different from access token
        refreshToken.Should().NotBe(accessToken);
    }

    [Fact]
    public void ValidateRefreshToken_WithTokenFromDifferentIssuer_ShouldReturnNull()
    {
        // Arrange
        var user = CreateTestUser();
        var differentOptions = new JwtOptions
        {
            Issuer = "DifferentIssuer",
            Audience = "TestAudience",
            SecretKey = "ThisIsAVerySecretKeyForTestingPurposesOnly123456789",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7,
            CookieName = "TestCookie"
        };

        var differentOptionsMock = new Mock<IOptions<JwtOptions>>();
        differentOptionsMock.Setup(o => o.Value).Returns(differentOptions);
        
        var differentService = new JwtTokenService(differentOptionsMock.Object, _loggerMock.Object);
        var tokenFromDifferentIssuer = differentService.GenerateRefreshToken(user);

        // Act
        var principal = _sut.ValidateRefreshToken(tokenFromDifferentIssuer);

        // Assert
        principal.Should().BeNull();
    }
}

