using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Carts;

public record GetCartQuery(Guid UserId) : IRequest<Cart>;
