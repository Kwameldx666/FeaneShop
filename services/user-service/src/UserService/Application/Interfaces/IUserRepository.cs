using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Interfaces;

public interface IUserRepository
{
    IEnumerable<UserData> GetAllUsers();
    Task<OperationResult<UserProfile>> GetOneUserByIdAsync(Guid id);
    Task<OperationResult<UserProfile>> UpdateUser(UserData userNew);
    OperationResult<UserProfile> DeleteUser(Guid id);
    IEnumerable<UserData> FindUsersByName(string name);
    IEnumerable<Role> GetUserRoles(Guid id);
    OperationResult<UserProfile> IsUserExists(Guid id);
    OperationResult<UserProfile> AssignRoleToUser(Guid userId, Role role);
    OperationResult<UserProfile> DeactivateUser(Guid id);
    Task<OperationResult<UserProfile>> UpdateAddress(Guid userId, DeliveryAddress newAddress);
    Task<OperationResult<DeliveryAddress>> GetOneAddressByUserIdAsync(Guid userId);
}
