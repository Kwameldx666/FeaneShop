using AuthService.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserService.Controllers;
using UserService.Infrastructure.Persistence;

namespace UserService.Tests.Controllers;

public class UsersControllerTests
{
    private readonly UserDbContext _context;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        // Arrange - Create InMemory database
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new UserDbContext(options);
        _controller = new UsersController(_context);
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Username = "user1", Email = "user1@test.com" },
            new() { Id = Guid.NewGuid(), Username = "user2", Email = "user2@test.com" }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUsers();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", Email = "test@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _controller.GetUser(invalidId);

        // Assert
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CreateUser_WithValidData_CreatesUser()
    {
        // Arrange
        var newUser = new User
        {
            Username = "newuser",
            Email = "new@test.com",
            PasswordHash = "hashedpassword"
        };

        // Act
        var result = await _controller.CreateUser(newUser);

        // Assert
        result.Should().NotBeNull();
        var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateUser_WithValidData_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "oldname", Email = "old@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.Username = "newname";
        user.Email = "new@test.com";

        // Act
        var result = await _controller.UpdateUser(userId, user);

        // Assert
        result.Should().NotBeNull();
        var updatedUser = await _context.Users.FindAsync(userId);
        updatedUser!.Username.Should().Be("newname");
        updatedUser.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task DeleteUser_WithValidId_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "deleteuser", Email = "delete@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        result.Should().NotBeNull();
        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.Should().BeNull();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}