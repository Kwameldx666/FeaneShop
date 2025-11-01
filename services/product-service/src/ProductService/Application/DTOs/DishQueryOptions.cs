namespace ProductService.Application.DTOs;

public class DishQueryOptions
{
    public string? Category { get; set; }
    public string? Search { get; set; }
    public bool AvailableOnly { get; set; }
    public DishSortField SortBy { get; set; } = DishSortField.CreatedAt;
    public bool Descending { get; set; }
    public int? Limit { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

public enum DishSortField
{
    None = 0,
    Name = 1,
    Price = 2,
    CreatedAt = 3,
    UpdatedAt = 4,
    Popularity = 5
}