using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Authentication;

public record GenerateJwtTokenQuery(UserData User) : IRequest<string>;
