using FeaneMVC.Application.Common.Interfaces.Persistence;
using MediatR;

namespace FeaneMVC.Application.Commands.Carts.Handlers;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCartItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || request.DishId == Guid.Empty)
        {
            return false;
        }

        var cart = await _unitOfWork.Carts.GetCartAsync(request.UserId, cancellationToken);
        if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
        {
            return false;
        }

        var itemToRemove = cart.CartItems.FirstOrDefault(item => item.DishId == request.DishId);
        if (itemToRemove == null)
        {
            return false;
        }

        cart.CartItems.Remove(itemToRemove);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
