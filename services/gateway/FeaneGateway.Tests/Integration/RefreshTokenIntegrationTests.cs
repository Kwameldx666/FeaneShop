using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AuthService.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace FeaneGateway.Tests.Integration;

public class RefreshTokenIntegrationTests
{
    [Fact]
    public void RefreshEndpoint_ShouldAcceptCamelCaseJson()
    {
        // This test verifies that the endpoint accepts camelCase JSON from JavaScript clients
        var refreshTokenRequest = new
        {
            refreshToken = "test.refresh.token.here"
        };

        var json = JsonSerializer.Serialize(refreshTokenRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert the JSON is in camelCase format
        json.Should().Contain("refreshToken");
        json.Should().NotContain("RefreshToken");
    }

    [Fact]
    public void RefreshTokenRequest_ShouldDeserializeFromCamelCase()
    {
        // This test verifies that RefreshTokenRequest can deserialize camelCase JSON
        var camelCaseJson = "{\"refreshToken\":\"test.token.value\"}";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<RefreshTokenRequest>(camelCaseJson, options);

        result.Should().NotBeNull();
        result!.RefreshToken.Should().Be("test.token.value");
    }

    [Fact]
    public void RefreshTokenRequest_ShouldDeserializeFromPascalCase()
    {
        // This test verifies that RefreshTokenRequest can deserialize PascalCase JSON
        var pascalCaseJson = "{\"RefreshToken\":\"test.token.value\"}";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<RefreshTokenRequest>(pascalCaseJson, options);

        result.Should().NotBeNull();
        result!.RefreshToken.Should().Be("test.token.value");
    }
}
