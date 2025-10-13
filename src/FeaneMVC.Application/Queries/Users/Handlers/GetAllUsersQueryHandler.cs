using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Interfaces;
using MediatR;

namespace FeaneMVC.Application.Queries.Users.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserData>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public Task<IEnumerable<UserData>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userRepository.GetAllUsers();
        return Task.FromResult(users);
    }
}
