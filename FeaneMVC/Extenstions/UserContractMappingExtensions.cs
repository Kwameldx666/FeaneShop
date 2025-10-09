using System.Collections.Generic;
using System.Linq;
using FeaneMVC.Contracts.Users;
using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Extenstions;

public static class UserContractMappingExtensions
{
    public static UserSummary ToSummary(this UserData user)
    {
        return new UserSummary
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = user.Roles,
            IsActive = user.IsActive
        };
    }

    public static IEnumerable<UserSummary> ToSummaryCollection(this IEnumerable<UserData> users)
    {
        return users.Select(ToSummary).ToList();
    }
}
