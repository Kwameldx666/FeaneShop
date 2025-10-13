using UserService.Models;

namespace UserService.Tests;

public class UserStoreTests
{
    [Fact]
    public void CreateUser_AssignsIdentifier()
    {
        var store = new InMemoryUserStore();
        var created = store.Create(new CreateUserRequest("example@example.com", "Feane User"));

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("example@example.com", created.Email);
    }
}
