namespace FeaneMVC.Contracts.Users;

public class UserManagementPageModel
{
    public IEnumerable<UserSummary> Users { get; set; } = new List<UserSummary>();

    public UserSummary? UserToEdit { get; set; }
}
