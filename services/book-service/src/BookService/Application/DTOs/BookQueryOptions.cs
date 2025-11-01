namespace BookService.Application.DTOs;

public class BookQueryOptions
{
    public string? Genre { get; set; }
    public string? Search { get; set; }
    public bool AvailableOnly { get; set; }
    public BookSortField SortBy { get; set; } = BookSortField.CreatedAt;
    public bool Descending { get; set; }
    public int? Limit { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

public enum BookSortField
{
    None = 0,
    Title = 1,
    Author = 2,
    Price = 3,
    PublishedOn = 4,
    CreatedAt = 5,
    UpdatedAt = 6
}