using FeaneMVC.Application.Commands.Sessions;
using FeaneMVC.Application.Commands.Users;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.Common.Security;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FeaneMVC.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;
        private readonly ILogger<SessionService> _logger;
        private readonly EmailAddressAttribute _emailValidator = new();

        public SessionService(IHttpContextAccessor httpContextAccessor, IMediator mediator, ILogger<SessionService> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> SetUserCookieAsync(Guid userId, string loginCredential, bool rememberMe, CancellationToken cancellationToken = default)
        {
            var cookieValue = CookieGenerator.Create(loginCredential);
            var loginTime = DateTime.UtcNow;

            var expireTime = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(60);

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("Unable to set user cookie because the HTTP context is unavailable.");
                return string.Empty;
            }

            var isHttps = httpContext.Request?.IsHttps ?? false;

            var cookieOptions = new CookieOptions
            {
                Expires = expireTime,
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            };

            httpContext.Response.Cookies.Append("X-KEY", cookieValue, cookieOptions);

            var isEmail = _emailValidator.IsValid(loginCredential);
            await _mediator.Send(new UpsertSessionCommand(loginCredential, cookieValue, expireTime, isEmail), cancellationToken);

            if (userId != Guid.Empty)
            {
                await _mediator.Send(new UpdateUserLoginAuditCommand(userId, cookieValue, loginTime), cancellationToken);
            }

            return cookieValue;
        }

        public UserData? GetUserByCookie(string cookieValue)
        {
            return _mediator.Send(new GetUserByCookieQuery(cookieValue)).GetAwaiter().GetResult();
        }

        public void UserLogout()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return;
            }

            httpContext.Response.Cookies.Delete("X-KEY");
            httpContext.Session.Clear();
        }

        public Guid GetUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var claimUserId = httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(claimUserId, out var claimId) && claimId != Guid.Empty)
            {
                return claimId;
            }

            var userIdString = httpContext?.Session.GetString("UserId");
            if (Guid.TryParse(userIdString, out var userId) && userId != Guid.Empty)
            {
                return userId;
            }

            userId = GetUserIdFromDatabase();
            if (userId == Guid.Empty)
            {
                return Guid.Empty;
            }

            SetSession("UserId", userId.ToString());
            return userId;
        }

        public async Task SessionStatus()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            var apiCookie = httpContext.Request.Cookies["X-KEY"];
            if (apiCookie == null)
            {
                httpContext.Session.SetString("LoginStatus", "logout");
                return;
            }

            var profile = await _mediator.Send(new GetUserByCookieQuery(apiCookie));
            if (profile != null)
            {
                httpContext.Session.SetString("LoginStatus", "login");
                httpContext.Session.SetString("Permission", profile.Roles.ToString());
            }
            else
            {
                httpContext.Session.Clear();

                if (httpContext.Request.Cookies.ContainsKey("X-KEY"))
                {
                    var cookie = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(-1),
                        HttpOnly = true,
                        Secure = httpContext.Request.IsHttps
                    };

                    httpContext.Response.Cookies.Append("X-KEY", string.Empty, cookie);
                }

                httpContext.Session.SetString("LoginStatus", "logout");
            }
        }

        public void SetSession(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Session name cannot be null or empty.", nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Session value cannot be null.");
            }

            _httpContextAccessor.HttpContext?.Session.SetString(name, value);
        }

        private Guid GetUserIdFromDatabase()
        {
            try
            {
                var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies["X-KEY"];
                if (string.IsNullOrWhiteSpace(cookieValue))
                {
                    return Guid.Empty;
                }

                var user = _mediator.Send(new GetUserByCookieQuery(cookieValue)).GetAwaiter().GetResult();
                return user?.Id ?? Guid.Empty;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to retrieve user id from database using cookie.");
                return Guid.Empty;
            }
        }
    }
}
