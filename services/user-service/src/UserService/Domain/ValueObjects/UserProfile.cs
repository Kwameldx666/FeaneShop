using UserService.Domain.Entities;

namespace UserService.Domain.ValueObjects;

public class UserProfile
{
    public UserData? User { get; set; }
    public DeliveryAddress? DeliveryAddress { get; set; }
    public bool AdminMod { get; set; }
    public bool ModeratorMod { get; set; }
}
