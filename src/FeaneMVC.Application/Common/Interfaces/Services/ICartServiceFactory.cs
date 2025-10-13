using FeaneMVC.Domain.Enums;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface ICartServiceFactory
{
    ICartService Resolve(Role role);
}
