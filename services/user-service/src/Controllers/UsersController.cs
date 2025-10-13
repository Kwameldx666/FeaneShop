using Microsoft.AspNetCore.Mvc;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserStore users) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<UserProfile>> ListUsers() => Ok(users.GetAll());

    [HttpGet("{id:guid}")]
    public ActionResult<UserProfile> GetUser(Guid id)
        => users.GetById(id) is { } profile ? Ok(profile) : NotFound();

    [HttpPost]
    public ActionResult<UserProfile> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            ModelState.AddModelError(nameof(request.Email), "Email is required.");
            return ValidationProblem(ModelState);
        }

        var created = users.Create(request);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }
}
