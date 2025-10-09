using FeaneMVC.Domain.Interfaces;
using MediatR;

namespace FeaneMVC.Application.Commands.Users.Handlers;

public class UpdateUserLoginAuditCommandHandler : IRequestHandler<UpdateUserLoginAuditCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserLoginAuditCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public Task<bool> Handle(UpdateUserLoginAuditCommand request, CancellationToken cancellationToken)
    {
        return _userRepository.UpdateUserLoginAuditAsync(request.UserId, request.CookieValue, request.LoginTime, cancellationToken);
    }
}
