using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Application.Mapping;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Queries.Dishes.Handlers;

public class GetDishByIdQueryHandler : IRequestHandler<GetDishByIdQuery, OperationResult<DishDto>>
{
    private readonly IDishReadRepository _dishReadRepository;

    public GetDishByIdQueryHandler(IDishReadRepository dishReadRepository)
    {
        _dishReadRepository = dishReadRepository;
    }

    public async Task<OperationResult<DishDto>> Handle(GetDishByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.DishId == Guid.Empty)
        {
            return OperationResult<DishDto>.Failure("Invalid dish ID");
        }

        var dish = await _dishReadRepository.GetByIdAsync(request.DishId, cancellationToken);
        if (dish == null)
        {
            return OperationResult<DishDto>.Failure("Dish not found");
        }

        return OperationResult<DishDto>.Success(dish.ToDishDto(), "Dish retrieved successfully");
    }
}
