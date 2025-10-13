using FeaneMVC.Application.Configuration;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feane.AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _users;
        private readonly JwtTokenService _jwt;

        public AuthController(UserRepository users, JwtTokenService jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserData model)
        {
            var result = _users.AddUser(model);
            if (!result.Status)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthenticationRequest request)
        {
            var result = _users.AuthenticateUser(request.Credential, request.Password);
            if (!result.Status || result.Data?.User == null)
                return Unauthorized(result.Message);

            var token = _jwt.GenerateToken(result.Data.User);
            return Ok(new { Token = token, User = result.Data.User });
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult Profile()
        {
            var idClaim = User.FindFirst("nameid")?.Value;
            return Ok(new { UserId = idClaim });
        }
    }
}
