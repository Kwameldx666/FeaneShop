using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Application.Mapping;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Dishes.Handlers;

public class CreateDishCommandHandler : IRequestHandler<CreateDishCommand, OperationResult<DishDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDishCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<OperationResult<DishDto>> Handle(CreateDishCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OperationResult<DishDto>.Failure("Dish name is required");
        }

        var dish = new Dish
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Category = request.Category.Trim(),
            ImageUrl = request.ImageUrl
        };

        var result = await _unitOfWork.DishWriter.AddAsync(dish, cancellationToken);
        if (!result.Status || result.Data is null)
        {
            return OperationResult<DishDto>.Failure(result.Message ?? "Failed to add dish");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<DishDto>.Success(result.Data.ToDishDto(), result.Message);
    }
}
