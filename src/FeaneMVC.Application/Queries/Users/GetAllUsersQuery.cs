using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Users;

public record GetAllUsersQuery() : IRequest<IEnumerable<UserData>>;
