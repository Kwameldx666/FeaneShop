using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(UserData user);
}
