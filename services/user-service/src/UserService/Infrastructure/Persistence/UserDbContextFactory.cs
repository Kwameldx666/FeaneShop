using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Infrastructure.Persistence;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=Feane.UserServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;");

        return new UserDbContext(optionsBuilder.Options);
    }
}