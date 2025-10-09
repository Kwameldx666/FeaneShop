using FeaneMVC.Application.DTOs.Dishes;
using MediatR;

namespace FeaneMVC.Application.Queries.Dishes;

public sealed record GetAllDishesQuery() : IRequest<IReadOnlyList<DishDto>>;
