using MediatR;

namespace FeaneMVC.Application.Commands.Carts;

public record RemoveCartItemCommand(Guid UserId, Guid DishId) : IRequest<bool>;
