using MediatR;

namespace FeaneMVC.Application.Commands.Sessions;

public record UserLogoutCommand() : IRequest<Unit>;
