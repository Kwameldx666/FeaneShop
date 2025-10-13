using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Sessions;

public record GetUserByCookieQuery(string CookieValue) : IRequest<UserData?>;
