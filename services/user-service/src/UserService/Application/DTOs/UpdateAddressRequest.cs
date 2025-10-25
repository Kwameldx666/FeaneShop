using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

public record UpdateAddressRequest(Guid UserId, DeliveryAddress Address);
