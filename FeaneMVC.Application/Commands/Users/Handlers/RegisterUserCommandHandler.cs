using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, OperationResult<UserProfile>>
{
    private readonly IUserRepository _userRepository;

    public RegisterUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public Task<OperationResult<UserProfile>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var result = _userRepository.AddUser(request.User);
        return Task.FromResult(result);
    }
}
