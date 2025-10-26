using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserData> Users => Set<UserData>();
    public DbSet<DeliveryAddress> DeliveryAddresses => Set<DeliveryAddress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserData>()
            .HasOne(u => u.Delivery)
            .WithOne(d => d.User)
            .HasForeignKey<DeliveryAddress>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserData>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique();

        modelBuilder.Entity<UserData>()
            .HasIndex(u => u.NormalizedUserName)
            .IsUnique();

        modelBuilder.Entity<UserData>()
            .HasIndex(u => u.AuthUserId)
            .IsUnique();
    }
}
