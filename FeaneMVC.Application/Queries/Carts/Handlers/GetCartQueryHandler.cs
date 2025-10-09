using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Carts.Handlers;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, Cart>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Cart> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return new Cart
            {
                CartId = Guid.NewGuid(),
                UserId = request.UserId,
                CartItems = new List<CartItem>()
            };
        }

        var cartRepository = _unitOfWork.Carts;

        var cart = await cartRepository.GetCartAsync(request.UserId, cancellationToken);
        if (cart != null)
        {
            cart.CartItems ??= new List<CartItem>();
            return cart;
        }

        cart = await cartRepository.GetOrCreateCartAsync(request.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        cart.CartItems ??= new List<CartItem>();
        return cart;
    }
}
