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

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OperationResult<UserProfile>>> UpdateProfile(Guid id, [FromBody] UserProfileUpdateRequest request)
    {
        if (!ModelState.IsValid || request == null)
        {
            return ValidationProblem(ModelState);
        }

        if (id != request.AuthUserId)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("User identifier mismatch."));
        }

        var profile = new UserData
        {
            Id = request.AuthUserId,
            AuthUserId = request.AuthUserId,
            Username = request.Username.Trim(),
            NormalizedUserName = request.Username.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
            Roles = request.Role,
            IsActive = request.IsActive
        };

        var result = await _userRepository.UpdateUser(profile);
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

    [HttpGet("{id:guid}/roles")]
    public ActionResult<IEnumerable<Role>> GetUserRoles(Guid id)
    {
        var roles = _userRepository.GetUserRoles(id);
        return Ok(roles);
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

    [HttpPut("{id:guid}/address")]
    public async Task<ActionResult<OperationResult<UserProfile>>> UpdateAddress(Guid id, [FromBody] DeliveryAddress address)
    {
        if (address == null)
        {
            return BadRequest(OperationResult<UserProfile>.Failure("Address payload is required."));
        }

        var result = await _userRepository.UpdateAddress(id, address);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id:guid}/address")]
    public async Task<ActionResult<OperationResult<DeliveryAddress>>> GetAddress(Guid id)
    {
        var result = await _userRepository.GetOneAddressByUserIdAsync(id);
        return result.Status ? Ok(result) : NotFound(result);
    }
}
