using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace FeaneMVC.Application.Services.Factories
{
    /// <summary>
    /// Default implementation of <see cref="ICartServiceFactory"/> that relies on the dependency injection container.
    /// </summary>
    public sealed class CartFactory : ICartServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CartFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ICartService Resolve(Role role)
        {
            return role switch
            {
                Role.VIP => _serviceProvider.GetRequiredService<VIPUserCartService>(),
                _ => _serviceProvider.GetRequiredService<RegularUserCartService>()
            };
        }
    }
}
