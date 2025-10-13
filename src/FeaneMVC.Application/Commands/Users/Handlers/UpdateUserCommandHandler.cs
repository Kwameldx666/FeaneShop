using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, OperationResult<UserProfile>>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public Task<OperationResult<UserProfile>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        return _userRepository.UpdateUser(request.User);
    }
}
