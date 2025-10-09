using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Persistence;

public interface ISessionRepository
{
    Task<Session?> FindByCredentialAsync(string credential, bool isEmail, CancellationToken cancellationToken = default);

    Task AddSessionAsync(Session session, CancellationToken cancellationToken = default);

    Task<UserData?> GetUserByCookieAsync(string cookieValue, CancellationToken cancellationToken = default);
}
