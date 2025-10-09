using FeaneMVC.Application.Common.Interfaces.Persistence;
using MediatR;

namespace FeaneMVC.Application.Commands.Carts.Handlers;

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemQuantityCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || request.DishId == Guid.Empty)
        {
            return false;
        }

        var cart = await _unitOfWork.Carts.GetCartAsync(request.UserId, cancellationToken);
        if (cart == null || cart.CartItems == null)
        {
            return false;
        }

        var itemToUpdate = cart.CartItems.FirstOrDefault(item => item.DishId == request.DishId);
        if (itemToUpdate == null)
        {
            return false;
        }

        var quantity = Math.Max(request.Quantity, 1);
        itemToUpdate.Quantity = quantity;
        itemToUpdate.TotalPrice = itemToUpdate.Price * quantity;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
