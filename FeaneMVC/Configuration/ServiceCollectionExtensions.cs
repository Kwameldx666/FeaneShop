using FeaneMVC.Application;
using FeaneMVC.Application.Configuration;
using FeaneMVC.Attributes;
using FeaneMVC.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FeaneMVC.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection(JwtOptions.SectionName);
            services.Configure<JwtOptions>(jwtSection);

            var jwtOptions = jwtSection.Get<JwtOptions>();
            if (jwtOptions is null || !jwtOptions.IsValid())
            {
                throw new InvalidOperationException("JWT settings are missing or invalid.");
            }

            var authenticationBuilder = services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (string.IsNullOrEmpty(context.Token)
                                && context.Request.Cookies.TryGetValue(jwtOptions.CookieName, out var token)
                                && !string.IsNullOrWhiteSpace(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            if (context.Response.HasStarted)
                            {
                                return Task.CompletedTask;
                            }

                            var requestPath = context.Request.Path;
                            if (requestPath.HasValue
                                && requestPath.StartsWithSegments("/Account/Authentication", StringComparison.OrdinalIgnoreCase))
                            {
                                // Avoid redirect loops when the unauthenticated request is already targeting the authentication page.
                                return Task.CompletedTask;
                            }

                            context.HandleResponse();

                            if (RequestExpectsJson(context.Request))
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
                            }

                            var returnUrl = BuildReturnUrl(context.Request);
                            var queryParameters = new Dictionary<string, string?>();

                            if (!string.IsNullOrEmpty(returnUrl)
                                && returnUrl != "/"
                                && !requestPath.StartsWithSegments("/Account/Authentication", StringComparison.OrdinalIgnoreCase))
                            {
                                queryParameters["returnUrl"] = returnUrl;
                            }

                            var redirectUrl = QueryHelpers.AddQueryString("/Account/Authentication", queryParameters);
                            context.Response.Redirect(redirectUrl);
                            return Task.CompletedTask;
                        }
                    };
                });

            authenticationBuilder.AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = "Feane.Identity";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/Account/Authentication";
                options.AccessDeniedPath = "/Account/Authentication";
                options.SlidingExpiration = true;
            });

            authenticationBuilder.AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = "Feane.Identity.External";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

            authenticationBuilder.AddCookie(IdentityConstants.TwoFactorUserIdScheme, options =>
            {
                options.Cookie.Name = "Feane.Identity.TwoFactor";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

            authenticationBuilder.AddCookie(IdentityConstants.TwoFactorRememberMeScheme, options =>
            {
                options.Cookie.Name = "Feane.Identity.TwoFactorRememberMe";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            services.AddAuthorization();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddApplication();
            services.AddInfrastructure(configuration);

            services.AddScoped<AdminOrModeratorModeAttribute>();
            services.AddScoped<AdminModeAttribute>();
            services.AddScoped<AdminOrVipModeAttribute>();
            services.AddScoped<VipModeAttribute>();
            services.AddScoped<ModeratorModeAttribute>();

            return services;
        }

        private static bool RequestExpectsJson(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
                && requestedWith.Any(value => string.Equals(value, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            var acceptHeader = request.Headers["Accept"].ToString();
            if (!string.IsNullOrWhiteSpace(acceptHeader)
                && acceptHeader.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            var contentType = request.ContentType;
            if (!string.IsNullOrWhiteSpace(contentType)
                && contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return request.Path.StartsWithSegments("/api");
        }

        private static string BuildReturnUrl(HttpRequest request)
        {
            if (request == null)
            {
                return "/";
            }

            var path = request.Path.HasValue ? request.Path.Value : "/";
            var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

            var combined = string.Concat(path, query);
            if (string.IsNullOrWhiteSpace(combined))
            {
                return "/";
            }

            return combined;
        }
    }
}
