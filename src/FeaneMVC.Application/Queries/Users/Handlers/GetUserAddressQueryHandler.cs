using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Interfaces;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Queries.Users.Handlers;

public class GetUserAddressQueryHandler : IRequestHandler<GetUserAddressQuery, OperationResult<DeliveryAddress>>
{
    private readonly IUserRepository _userRepository;

    public GetUserAddressQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<OperationResult<DeliveryAddress>> Handle(GetUserAddressQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.GetOneAddressByUserIdAsync(request.UserId);
    }
}
