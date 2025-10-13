using Microsoft.AspNetCore.Http;

namespace FeaneMVC.Services;

public class UserSessionAccessor : IUserSessionAccessor
{
    private const string UserIdKey = "Feane:UserId";

    public Task<Guid> GetOrCreateUserIdAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        if (httpContext.Session.TryGetValue(UserIdKey, out var value) && value.Length == 16)
        {
            var buffer = new byte[16];
            value.CopyTo(buffer, 0);
            return Task.FromResult(new Guid(buffer));
        }

        var userId = Guid.NewGuid();
        httpContext.Session.Set(UserIdKey, userId.ToByteArray());
        return Task.FromResult(userId);
    }
}
