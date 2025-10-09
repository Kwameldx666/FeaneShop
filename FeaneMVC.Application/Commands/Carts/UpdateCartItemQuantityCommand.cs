using MediatR;

namespace FeaneMVC.Application.Commands.Carts;

public record UpdateCartItemQuantityCommand(Guid UserId, Guid DishId, int Quantity) : IRequest<bool>;
