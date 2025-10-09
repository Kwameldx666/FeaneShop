using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SessionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Session?> FindByCredentialAsync(string credential, bool isEmail, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return null;
        }

        if (isEmail)
        {
            return await _dbContext.Sessions.FirstOrDefaultAsync(session => session.Email == credential, cancellationToken);
        }

        return await _dbContext.Sessions.FirstOrDefaultAsync(session => session.Username == credential, cancellationToken);
    }

    public Task AddSessionAsync(Session session, CancellationToken cancellationToken = default)
    {
        return _dbContext.Sessions.AddAsync(session, cancellationToken).AsTask();
    }

    public async Task<UserData?> GetUserByCookieAsync(string cookieValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            return null;
        }

        var session = await _dbContext.Sessions.AsNoTracking().FirstOrDefaultAsync(record => record.CookieString == cookieValue, cancellationToken);
        if (session == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(session.Username))
        {
            return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Username == session.Username, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(session.Email))
        {
            return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email == session.Email, cancellationToken);
        }

        return null;
    }

}
