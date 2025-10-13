using FeaneMVC.Application.Commands.Carts;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.Queries.Carts;
using FeaneMVC.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeaneMVC.Application.Services
{
    public class VIPUserCartService : ICartService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VIPUserCartService> _logger;

        public VIPUserCartService(IMediator mediator, ILogger<VIPUserCartService> logger)
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
                await _mediator.Send(new AddCartItemCommand(userId, item, true));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error adding VIP cart item for user {UserId}", userId);
            }
        }

        public async Task RemoveItemFromCartAsync(Guid userId, Guid dishId)
        {
            if (dishId == Guid.Empty)
            {
                throw new ArgumentException("Invalid dish ID", nameof(dishId));
            }

            try
            {
                await _mediator.Send(new RemoveCartItemCommand(userId, dishId));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing VIP cart item for user {UserId}", userId);
            }
        }

        public async Task UpdateItemQuantityAsync(Guid userId, Guid dishId, int quantity)
        {
            if (quantity < 1)
            {
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            }

            try
            {
                await _mediator.Send(new UpdateCartItemQuantityCommand(userId, dishId, quantity));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error updating VIP cart item for user {UserId}", userId);
            }
        }

        public async Task<Cart> GetCartAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Invalid user ID", nameof(userId));
            }

            try
            {
                var cart = await _mediator.Send(new GetCartQuery(userId));
                if (cart != null)
                {
                    return cart;
                }

                return new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error retrieving VIP cart for user {UserId}", userId);
                throw;
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
                _logger.LogError(exception, "Error calculating VIP cart total for user {UserId}", userId);
                return 0m;
            }
        }
    }
}
