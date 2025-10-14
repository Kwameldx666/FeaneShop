namespace UserService.Domain.Entities;

public class DeliveryAddress
{
    public Guid Id { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ParcelIndex { get; set; }
    public string? MoreInfo { get; set; }
    public UserData? User { get; set; }
    public Guid UserId { get; set; }
}
