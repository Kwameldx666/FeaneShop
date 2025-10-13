using AuthService.Application.Configuration;
using Microsoft.Extensions.Options;

namespace AuthService.Middleware;

public sealed class JwtCookieAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtOptions _options;

    public JwtCookieAuthenticationMiddleware(RequestDelegate next, IOptions<JwtOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (!_options.IsValid())
        {
            throw new InvalidOperationException("JWT settings are not configured correctly.");
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Headers.ContainsKey("Authorization")
            && context.Request.Cookies.TryGetValue(_options.CookieName, out var token)
            && !string.IsNullOrWhiteSpace(token))
        {
            context.Request.Headers.Authorization = $"Bearer {token}";
        }

        await _next(context);
    }
}

public static class JwtCookieAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtCookieAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtCookieAuthenticationMiddleware>();
    }
}
