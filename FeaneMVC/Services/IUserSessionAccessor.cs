using Microsoft.AspNetCore.Http;

namespace FeaneMVC.Services;

public interface IUserSessionAccessor
{
    Task<Guid> GetOrCreateUserIdAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}
