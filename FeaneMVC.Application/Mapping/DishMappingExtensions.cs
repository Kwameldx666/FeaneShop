using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Mapping;

public static class DishMappingExtensions
{
    public static DishDto ToDishDto(this Dish dish)
    {
        if (dish == null)
        {
            throw new ArgumentNullException(nameof(dish));
        }

        return new DishDto
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

    public static IReadOnlyList<DishDto> ToDishDtoList(this IEnumerable<Dish> dishes)
    {
        return dishes?.Select(ToDishDto).ToList() ?? new List<DishDto>();
    }
}
