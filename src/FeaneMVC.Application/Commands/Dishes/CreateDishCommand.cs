using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Dishes;

public sealed record CreateDishCommand(
    string Name,
    string Description,
    decimal Price,
    string Category,
    string? ImageUrl) : IRequest<OperationResult<DishDto>>;
