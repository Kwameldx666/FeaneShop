using MediatR;

namespace FeaneMVC.Application.Commands.Sessions;

public record SetSessionValueCommand(string Key, string Value) : IRequest<Unit>;
