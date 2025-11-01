using ReservationService.Domain.Enums;

namespace ReservationService.Application.DTOs;

public class ReservationQueryOptions
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public ReservationStatus? Status { get; set; }
    public bool UpcomingOnly { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Sort { get; set; }
    public bool Descending { get; set; }
}