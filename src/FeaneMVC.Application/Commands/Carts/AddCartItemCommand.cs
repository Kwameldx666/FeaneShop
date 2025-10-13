using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Commands.Carts;

public record AddCartItemCommand(Guid UserId, CartItem Item, bool ApplyVipDiscount) : IRequest<bool>;
