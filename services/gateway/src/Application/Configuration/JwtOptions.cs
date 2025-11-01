namespace AuthService.Application.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationDays { get; set; } = 7;

    public string CookieName { get; set; } = "AuthToken";

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Issuer)
               && !string.IsNullOrWhiteSpace(Audience)
               && !string.IsNullOrWhiteSpace(SecretKey)
               && AccessTokenExpirationMinutes > 0
               && !string.IsNullOrWhiteSpace(CookieName);
    }
}