using FeaneMVC.Application.Common.Interfaces.Persistence;
using MediatR;

namespace FeaneMVC.Application.Queries.Carts.Handlers;

public class CalculateCartTotalQueryHandler : IRequestHandler<CalculateCartTotalQuery, decimal>
{
    private readonly ICartRepository _cartRepository;

    public CalculateCartTotalQueryHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
    }

    public async Task<decimal> Handle(CalculateCartTotalQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return 0m;
        }

        var cart = await _cartRepository.GetCartAsync(request.UserId, cancellationToken);
        if (cart?.CartItems == null)
        {
            return 0m;
        }

        return cart.CartItems.Sum(item => item.TotalPrice);
    }
}
