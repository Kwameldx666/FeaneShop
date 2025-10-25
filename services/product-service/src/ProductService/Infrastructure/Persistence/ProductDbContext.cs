using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Dish> Dishes => Set<Dish>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Dish>(entity =>
        {
            entity.ToTable("Dishes");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(128);
            entity.Property(d => d.Description).IsRequired().HasMaxLength(2048);
            entity.Property(d => d.Category).IsRequired().HasMaxLength(64);
            entity.Property(d => d.Price).HasPrecision(10, 2);
            entity.Property(d => d.ImageBase64).HasColumnType("nvarchar(max)");
            entity.Property(d => d.ImageMimeType).HasMaxLength(128);
            entity.Property(d => d.IsAvailable).HasDefaultValue(true);
            entity.Property(d => d.IsFeatured).HasDefaultValue(false);
            entity.Property(d => d.PopularityScore).HasDefaultValue(0);
            entity.Property(d => d.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(d => d.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(d => d.Category);
            entity.HasIndex(d => d.IsAvailable);
            entity.HasIndex(d => d.IsFeatured);
        });

        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Dish>().HasData(
            new Dish
            {
                Id = Guid.Parse("3F1A3C78-556C-4E8B-B9DD-5E72D6A5D82F"),
                Name = "Classic Burger",
                Description = "Juicy beef patty with cheddar cheese, lettuce, tomato and signature sauce.",
                Price = 12.50m,
                Category = "burger",
                IsAvailable = true,
                IsFeatured = true,
                PopularityScore = 85,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Dish
            {
                Id = Guid.Parse("E5B28A3D-ED0C-4F4F-A26A-F879C0DEB9C6"),
                Name = "Spicy Chicken Pizza",
                Description = "Thin crust pizza topped with spicy chicken, mozzarella and jalapeños.",
                Price = 15.90m,
                Category = "pizza",
                IsAvailable = true,
                IsFeatured = true,
                PopularityScore = 92,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Dish
            {
                Id = Guid.Parse("8C41C36C-C5C6-49DF-B3B7-16E785AC6F75"),
                Name = "Creamy Mushroom Pasta",
                Description = "Tagliatelle tossed in a creamy mushroom sauce with parmesan flakes.",
                Price = 13.40m,
                Category = "pasta",
                IsAvailable = true,
                IsFeatured = false,
                PopularityScore = 74,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
