using FeaneMVC.Application.Common.Behaviors;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.Services;
using FeaneMVC.Application.Services.Factories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using INotification = FeaneMVC.Application.Common.Interfaces.Services.INotification;

namespace FeaneMVC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<NotificationService>());

        services.AddValidatorsFromAssemblyContaining<NotificationService>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<RegularUserCartService>();
        services.AddScoped<VIPUserCartService>();
        services.AddScoped<ICartServiceFactory, CartFactory>();
        services.AddScoped<IPaymentGateway, PaymentProcessor>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddSingleton<INotification, NotificationService>();

        return services;
    }
}
