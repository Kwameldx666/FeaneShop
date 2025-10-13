using MediatR;

namespace FeaneMVC.Application.Queries.Carts;

public record CalculateCartTotalQuery(Guid UserId) : IRequest<decimal>;
