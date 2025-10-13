using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users.Handlers;

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, OperationResult<UserProfile>>
{
    private readonly IUserRepository _userRepository;

    public AuthenticateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public Task<OperationResult<UserProfile>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var result = _userRepository.AuthenticateUser(request.Credential, request.Password);
        return Task.FromResult(result);
    }
}
