using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserData>> GetAllUsers()
    {
        var users = _userRepository.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OperationResult<UserProfile>>> GetUserById(Guid id)
    {
        var result = await _userRepository.GetOneUserByIdAsync(id);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public ActionResult<OperationResult<UserProfile>> CreateUser([FromBody] UserData user)
    {
        if (user == null)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("User payload is required."));
        }

        var result = _userRepository.AddUser(user);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OperationResult<UserProfile>>> UpdateUser(Guid id, [FromBody] UserData user)
    {
        if (user == null || id != user.Id)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("User identifier mismatch."));
        }

        var result = await _userRepository.UpdateUser(user);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}")]
    public ActionResult<OperationResult<UserProfile>> DeleteUser(Guid id)
    {
        var result = _userRepository.DeleteUser(id);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<UserData>> FindUsersByName([FromQuery] string name)
    {
        var users = _userRepository.FindUsersByName(name);
        return Ok(users);
    }

    [HttpPost("authenticate")]
    public ActionResult<OperationResult<UserProfile>> Authenticate([FromBody] AuthenticateRequest request)
    {
        if (request == null)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("Authentication payload is required."));
        }

        var result = _userRepository.AuthenticateUser(request.Credential, request.Password);
        return result.Status ? Ok(result) : Unauthorized(result);
    }

    [HttpGet("{id:guid}/roles")]
    public ActionResult<IEnumerable<Role>> GetUserRoles(Guid id)
    {
        var roles = _userRepository.GetUserRoles(id);
        return Ok(roles);
    }

    [HttpPost("change-password")]
    public ActionResult<OperationResult<UserProfile>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request == null)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("Email is required."));
        }

        var result = _userRepository.ChangeUserPassword(request.Email);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id:guid}/exists")]
    public ActionResult<OperationResult<UserProfile>> IsUserExists(Guid id)
    {
        var result = _userRepository.IsUserExists(id);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:guid}/assign-role")]
    public ActionResult<OperationResult<UserProfile>> AssignRole(Guid id, [FromBody] AssignRoleRequest request)
    {
        if (request == null)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("Role payload is required."));
        }

        var result = _userRepository.AssignRoleToUser(id, request.Role);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    public ActionResult<OperationResult<UserProfile>> Deactivate(Guid id)
    {
        var result = _userRepository.DeactivateUser(id);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id:guid}/address")]
    public async Task<ActionResult<OperationResult<DeliveryAddress>>> GetAddress(Guid id)
    {
        var result = await _userRepository.GetOneAddressByUserIdAsync(id);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}/address")]
    public async Task<ActionResult<OperationResult<UserProfile>>> UpdateAddress(Guid id, [FromBody] DeliveryAddress address)
    {
        if (address == null)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("Address payload is required."));
        }

        var user = new UserData { Id = id };
        var result = await _userRepository.UpdateAddress(user, address);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpPost("user-data")]
    public ActionResult<OperationResult<UserProfile>> GetUserData([FromBody] UserCredentialRequest request)
    {
        if (request == null)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("Credential payload is required."));
        }

        var user = new UserData { Credential = request.Credential, Password = request.Password };
        var result = _userRepository.GetUserData(user);
        return result.Status ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("by-cookie")]
    public async Task<ActionResult<UserData?>> GetUserByCookie([FromBody] CookieRequest request)
    {
        if (request == null)
        {
            return BadRequest("Cookie payload is required.");
        }

        var user = await _userRepository.GetUserByCookie(request.CookieValue);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost("logout")]
    public ActionResult<OperationResult> Logout()
    {
        var result = _userRepository.UserLogout();
        return Ok(result);
    }

    [HttpPost("login-audit")]
    public async Task<ActionResult> UpdateLoginAudit([FromBody] LoginAuditRequest request)
    {
        if (request == null)
        {
            return BadRequest("Login audit payload is required.");
        }

        var updated = await _userRepository.UpdateUserLoginAuditAsync(request.UserId, request.CookieValue, request.LoginTime);
        return updated ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }
}
