using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Queries.Dishes;

public sealed record GetDishByIdQuery(Guid DishId) : IRequest<OperationResult<DishDto>>;
