using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Dishes
{
    public sealed record DeleteDishCommand(Guid DishId) : IRequest<OperationResult>;
}
