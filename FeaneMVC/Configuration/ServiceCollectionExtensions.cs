using FeaneMVC.Clients.Menu;
using FeaneMVC.Clients.Reservations;
using FeaneMVC.Options;
using FeaneMVC.Services;
using Microsoft.Extensions.Options;

namespace FeaneMVC.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceEndpointsOptions>(configuration.GetSection(ServiceEndpointsOptions.SectionName));

        services.AddHttpClient<IMenuApiClient, MenuApiClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
            client.BaseAddress = new Uri(options.MenuService);
        });

        services.AddHttpClient<IReservationApiClient, ReservationApiClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<ServiceEndpointsOptions>>().Value;
            client.BaseAddress = new Uri(options.ReservationService);
        });

        services.AddScoped<IUserSessionAccessor, UserSessionAccessor>();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        return services;
    }
}
