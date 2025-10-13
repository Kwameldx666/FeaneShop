using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface ISessionService
{
    Task<string> SetUserCookieAsync(Guid userId, string loginCredential, bool rememberMe, CancellationToken cancellationToken = default);

    UserData? GetUserByCookie(string cookieValue);

    void UserLogout();

    Guid GetUserId();

    Task SessionStatus();

    void SetSession(string name, string value);
}
