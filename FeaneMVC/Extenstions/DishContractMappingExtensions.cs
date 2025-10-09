using FeaneMVC.Application.Commands.Dishes;
using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Contracts.Dishes;

namespace FeaneMVC.Extenstions;

public static class DishContractMappingExtensions
{
    public static DishResponse ToResponse(this DishDto dish)
    {
        return new DishResponse
        {
            Id = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Price = dish.Price,
            Category = dish.Category,
            ImageUrl = dish.ImageUrl,
            CreatedAt = dish.CreatedAt,
            UpdatedAt = dish.UpdatedAt
        };
    }

    public static IEnumerable<DishResponse> ToResponseCollection(this IEnumerable<DishDto> dishes)
    {
        return dishes.Select(ToResponse);
    }

    public static CreateDishCommand ToCommand(this CreateDishRequest request)
    {
        return new CreateDishCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.ImageUrl);
    }

    public static UpdateDishCommand ToCommand(this UpdateDishRequest request)
    {
        return new UpdateDishCommand(
            request.Id,
            request.Name,
            request.Description,
            request.Price,
            request.Category,
            request.ImageUrl);
    }

    public static UpdateDishRequest ToUpdateRequest(this DishResponse response)
    {
        return new UpdateDishRequest
        {
            Id = response.Id,
            Name = response.Name,
            Description = response.Description,
            Price = response.Price,
            Category = response.Category,
            ImageUrl = response.ImageUrl
        };
    }
}
