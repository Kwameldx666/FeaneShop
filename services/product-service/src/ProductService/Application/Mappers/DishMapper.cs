using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappers;

public static class DishMapper
{
    public static DishResponse ToResponse(this Dish dish)
    {
        if (dish == null)
        {
            throw new ArgumentNullException(nameof(dish));
        }

        var imageUrl = string.IsNullOrWhiteSpace(dish.ImageBase64)
            ? "/images/Default.png"
            : $"data:{dish.ImageMimeType ?? "image/png"};base64,{dish.ImageBase64}";

        return new DishResponse(
            dish.Id,
            dish.Name,
            dish.Description,
            dish.Price,
            dish.Category,
            imageUrl,
            dish.IsAvailable,
            dish.IsFeatured,
            dish.PopularityScore,
            dish.CreatedAt,
            dish.UpdatedAt);
    }
}
