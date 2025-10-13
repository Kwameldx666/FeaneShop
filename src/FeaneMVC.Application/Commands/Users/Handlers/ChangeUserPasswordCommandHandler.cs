using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users.Handlers;

public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, OperationResult<UserProfile>>
{
    private readonly IUserRepository _userRepository;

    public ChangeUserPasswordCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public Task<OperationResult<UserProfile>> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = _userRepository.ChangeUserPassword(request.Email);
        return Task.FromResult(result);
    }
}
