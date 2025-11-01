using BookService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookService.Infrastructure.Persistence;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(256);
            entity.Property(b => b.Author).IsRequired().HasMaxLength(128);
            entity.Property(b => b.Description).IsRequired().HasMaxLength(4096);
            entity.Property(b => b.Genre).IsRequired().HasMaxLength(64);
            entity.Property(b => b.Isbn).HasMaxLength(32);
            entity.Property(b => b.Price).HasPrecision(10, 2);
            entity.Property(b => b.CoverImageBase64).HasColumnType("nvarchar(max)");
            entity.Property(b => b.CoverImageMimeType).HasMaxLength(128);
            entity.Property(b => b.IsAvailable).HasDefaultValue(true);
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(b => b.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(b => b.Genre);
            entity.HasIndex(b => b.IsAvailable);
            entity.HasIndex(b => b.Author);
        });

        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Book book1 = new()
        {
            Id = Guid.Parse("C82D9D87-A518-47EE-B6F8-7AF7E9EDE91A"),
            Title = "Clean Architecture",
            Author = "Robert C. Martin",
            Description = "A Craftsman's Guide to Software Structure and Design.",
            Genre = "software",
            Price = 42.50m,
            Isbn = "9780134494166",
            PublishedOn = new DateTime(2017, 9, 20),
            IsAvailable = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        Book book2 = new()
        {
            Id = Guid.Parse("35B50074-8B94-4C95-9A6A-EBB0E0D49AC8"),
            Title = "The Pragmatic Programmer",
            Author = "Andrew Hunt, David Thomas",
            Description = "Journey to Mastery with practical advice for modern developers.",
            Genre = "software",
            Price = 39.90m,
            Isbn = "9780135957059",
            PublishedOn = new DateTime(2019, 9, 13),
            IsAvailable = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        Book book3 = new()
        {
            Id = Guid.Parse("8907A7A7-99F5-4CD7-8A74-8B7CAF34A61C"),
            Title = "Dune",
            Author = "Frank Herbert",
            Description = "Epic science fiction saga set on the desert planet Arrakis.",
            Genre = "science-fiction",
            Price = 18.75m,
            Isbn = "9780441172719",
            PublishedOn = new DateTime(1965, 8, 1),
            IsAvailable = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        modelBuilder.Entity<Book>().HasData(book1, book2, book3);
    }
}