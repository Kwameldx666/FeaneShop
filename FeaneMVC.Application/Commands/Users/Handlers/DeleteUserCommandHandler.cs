using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, OperationResult<UserProfile>>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<OperationResult<UserProfile>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        return _userRepository.DeleteUser(request.UserId);
    }
}
