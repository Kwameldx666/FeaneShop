using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Users.Handlers;

public class UpdateUserAddressCommandHandler : IRequestHandler<UpdateUserAddressCommand, OperationResult<UserProfile>>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserAddressCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public Task<OperationResult<UserProfile>> Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
    {
        return _userRepository.UpdateAddress(request.User, request.Address);
    }
}
