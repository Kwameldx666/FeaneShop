using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Application.Mapping;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Dishes.Handlers;

public class UpdateDishCommandHandler : IRequestHandler<UpdateDishCommand, OperationResult<DishDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDishCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<OperationResult<DishDto>> Handle(UpdateDishCommand request, CancellationToken cancellationToken)
    {
        if (request.DishId == Guid.Empty)
        {
            return OperationResult<DishDto>.Failure("Invalid dish identifier");
        }

        var dish = new Dish
        {
            Id = request.DishId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Category = request.Category.Trim(),
            ImageUrl = request.ImageUrl
        };

        var result = await _unitOfWork.DishWriter.UpdateAsync(request.DishId, dish, cancellationToken);
        if (!result.Status || result.Data is null)
        {
            return OperationResult<DishDto>.Failure(result.Message ?? "Failed to update dish");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<DishDto>.Success(result.Data.ToDishDto(), result.Message);
    }
}
