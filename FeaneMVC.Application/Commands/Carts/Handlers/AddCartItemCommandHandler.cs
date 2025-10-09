using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Commands.Carts.Handlers;

public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddCartItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        if (request.Item == null || request.UserId == Guid.Empty)
        {
            return false;
        }

        var cart = await _unitOfWork.Carts.GetOrCreateCartAsync(request.UserId, cancellationToken);
        cart.CartItems ??= new List<CartItem>();

        var quantity = Math.Max(request.Item.Quantity, 1);
        var price = request.ApplyVipDiscount ? ApplyVipDiscount(request.Item.Price) : request.Item.Price;

        var existingItem = cart.CartItems.FirstOrDefault(item => item.DishId == request.Item.DishId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            existingItem.Price = price;
            existingItem.TotalPrice = price * existingItem.Quantity;
            existingItem.Name = request.Item.Name;
            existingItem.UserId = request.UserId;
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.CartId,
                Cart = cart,
                DishId = request.Item.DishId,
                Dish = request.Item.Dish,
                UserId = request.UserId,
                Quantity = quantity,
                Price = price,
                TotalPrice = price * quantity,
                Name = request.Item.Name,
                User = request.Item.User
            };

            cart.CartItems.Add(newItem);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static decimal ApplyVipDiscount(decimal price) => price * 0.9m;
}
