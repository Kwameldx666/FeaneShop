using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Interfaces;

public interface IUserRepository
{
    IEnumerable<UserData> GetAllUsers();
    Task<OperationResult<UserProfile>> GetOneUserByIdAsync(Guid id);
    OperationResult<UserProfile> AddUser(UserData user);
    Task<OperationResult<UserProfile>> UpdateUser(UserData userNew);
    OperationResult<UserProfile> DeleteUser(Guid id);
    IEnumerable<UserData> FindUsersByName(string name);
    OperationResult<UserProfile> AuthenticateUser(string credential, string password);
    IEnumerable<Role> GetUserRoles(Guid id);
    OperationResult<UserProfile> ChangeUserPassword(string email);
    OperationResult<UserProfile> IsUserExists(Guid id);
    OperationResult<UserProfile> AssignRoleToUser(Guid userId, Role role);
    OperationResult<UserProfile> DeactivateUser(Guid id);
    OperationResult<UserProfile> GetUserData(UserData loginData);
    Task<UserData?> GetUserByCookie(string value);
    OperationResult UserLogout();
    Task<OperationResult<UserProfile>> UpdateAddress(UserData addressOld, DeliveryAddress newAddress);
    Task<OperationResult<DeliveryAddress>> GetOneAddressByUserIdAsync(Guid userId);
    Task<bool> UpdateUserLoginAuditAsync(Guid userId, string cookieValue, DateTime loginTime, CancellationToken cancellationToken = default);
}
