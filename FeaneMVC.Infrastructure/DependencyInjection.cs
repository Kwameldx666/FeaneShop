using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Infrastructure.Identity;
using FeaneMVC.Infrastructure.Persistence;
using FeaneMVC.Infrastructure.Persistence.Db;
using FeaneMVC.Infrastructure.Persistence.Repositories;
using FeaneMVC.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeaneMVC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpContextAccessor();
        services.AddHttpClient();

        services.AddScoped<IDishReadRepository, DishRepository>();
        services.AddScoped<IDishWriteRepository, DishRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IReservation, ReservationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        var identityBuilder = services.AddIdentityCore<UserData>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        });

        identityBuilder = identityBuilder.AddUserStore<UserStore>();
        identityBuilder = identityBuilder.AddSignInManager();
        identityBuilder = identityBuilder.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();
        identityBuilder.AddDefaultTokenProviders();

        services.AddScoped<IPasswordHasher<UserData>, LegacyPasswordHasher>();

        return services;
    }
}
