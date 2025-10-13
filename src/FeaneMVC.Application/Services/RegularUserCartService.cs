using FeaneMVC.Application.Commands.Carts;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.Queries.Carts;
using FeaneMVC.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeaneMVC.Application.Services
{
    public class RegularUserCartService : ICartService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RegularUserCartService> _logger;

        public RegularUserCartService(IMediator mediator, ILogger<RegularUserCartService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddItemToCartAsync(Guid userId, CartItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                await _mediator.Send(new AddCartItemCommand(userId, item, false));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error adding item to cart for user {UserId}", userId);
            }
        }

        public async Task RemoveItemFromCartAsync(Guid userId, Guid dishId)
        {
            try
            {
                await _mediator.Send(new RemoveCartItemCommand(userId, dishId));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing cart item for user {UserId}", userId);
            }
        }

        public async Task UpdateItemQuantityAsync(Guid userId, Guid dishId, int quantity)
        {
            try
            {
                await _mediator.Send(new UpdateCartItemQuantityCommand(userId, dishId, quantity));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error updating cart item quantity for user {UserId}", userId);
            }
        }

        public async Task<Cart> GetCartAsync(Guid userId)
        {
            try
            {
                var cart = await _mediator.Send(new GetCartQuery(userId));
                return cart ?? new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error retrieving cart for user {UserId}", userId);

                return new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
            }
        }

        public async Task<decimal> CalculateTotalAsync(Guid userId)
        {
            try
            {
                return await _mediator.Send(new CalculateCartTotalQuery(userId));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error calculating cart total for user {UserId}", userId);
                return 0m;
            }
        }
    }
}
