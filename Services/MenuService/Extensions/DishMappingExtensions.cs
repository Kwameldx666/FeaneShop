using Feane.Contracts.Dishes;
using MenuService.Models;
using System.Linq;

namespace MenuService.Extensions;

internal static class DishMappingExtensions
{
    public static DishResponse ToResponse(this DishDocument document)
    {
        return new DishResponse
        {
            Id = document.Id,
            Name = document.Name,
            Description = document.Description,
            Price = document.Price,
            Category = document.Category,
            ImageUrl = document.ImageUrl,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt ?? document.CreatedAt
        };
    }

    public static IEnumerable<DishResponse> ToResponseCollection(this IEnumerable<DishDocument> dishes)
    {
        return dishes.Select(ToResponse);
    }
}
