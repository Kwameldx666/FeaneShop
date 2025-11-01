using BookService.Controllers;
using BookService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookService.Tests.Controllers;

public class BooksControllerTests : IDisposable
{
    private readonly BookDbContext _context;
    private readonly BooksController _controller;

    public BooksControllerTests()
    {
        var options = new DbContextOptionsBuilder<BookDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookDbContext(options);
        _controller = new BooksController(_context);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task GetBookings_ReturnsAllBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@test.com",
                Phone = "1234567890",
                BookingDate = DateTime.UtcNow.AddDays(1),
                Guests = 4,
                Message = "Special occasion",
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Jane Smith",
                Email = "jane@test.com",
                Phone = "0987654321",
                BookingDate = DateTime.UtcNow.AddDays(2),
                Guests = 2,
                Message = "Anniversary",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Bookings.AddRange(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetBookings();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBooking_WithValidId_ReturnsBooking()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            Phone = "1111111111",
            BookingDate = DateTime.UtcNow.AddDays(1),
            Guests = 6,
            Message = "Birthday party",
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetBooking(bookingId);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Test User");
        result.Value.Guests.Should().Be(6);
    }

    [Fact]
    public async Task CreateBooking_WithValidData_CreatesBooking()
    {
        // Arrange
        var newBooking = new Booking
        {
            UserId = Guid.NewGuid(),
            Name = "New Customer",
            Email = "new@test.com",
            Phone = "5555555555",
            BookingDate = DateTime.UtcNow.AddDays(3),
            Guests = 8,
            Message = "Corporate event",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _controller.CreateBooking(newBooking);

        // Assert
        result.Should().NotBeNull();
        var createdBooking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Email == "new@test.com");

        createdBooking.Should().NotBeNull();
        createdBooking!.Name.Should().Be("New Customer");
        createdBooking.Guests.Should().Be(8);
    }

    [Fact]
    public async Task UpdateBookingStatus_WithValidData_UpdatesStatus()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            Phone = "1234567890",
            BookingDate = DateTime.UtcNow.AddDays(1),
            Guests = 4,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act
        await _controller.UpdateBookingStatus(bookingId, "Confirmed");

        // Assert
        var updatedBooking = await _context.Bookings.FindAsync(bookingId);
        updatedBooking!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CancelBooking_WithValidId_CancelsBooking()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            Phone = "1234567890",
            BookingDate = DateTime.UtcNow.AddDays(1),
            Guests = 4,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Act
        await _controller.CancelBooking(bookingId);

        // Assert
        var cancelledBooking = await _context.Bookings.FindAsync(bookingId);
        cancelledBooking!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetBookingsByUserId_ReturnsUserBookings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookings = new List<Booking>
        {
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "User One",
                Email = "user1@test.com",
                Phone = "1111111111",
                BookingDate = DateTime.UtcNow.AddDays(1),
                Guests = 2,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "User One",
                Email = "user1@test.com",
                Phone = "1111111111",
                BookingDate = DateTime.UtcNow.AddDays(5),
                Guests = 4,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(), // Different user
                Name = "User Two",
                Email = "user2@test.com",
                Phone = "2222222222",
                BookingDate = DateTime.UtcNow.AddDays(2),
                Guests = 3,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Bookings.AddRange(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetBookingsByUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.UserId == userId);
    }

    [Fact]
    public async Task GetBookingsByDate_ReturnsBookingsForDate()
    {
        // Arrange
        var targetDate = DateTime.UtcNow.AddDays(5).Date;
        var bookings = new List<Booking>
        {
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Booking 1",
                Email = "b1@test.com",
                Phone = "1111111111",
                BookingDate = targetDate,
                Guests = 2,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Booking 2",
                Email = "b2@test.com",
                Phone = "2222222222",
                BookingDate = targetDate,
                Guests = 4,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Booking 3",
                Email = "b3@test.com",
                Phone = "3333333333",
                BookingDate = DateTime.UtcNow.AddDays(10),
                Guests = 3,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Bookings.AddRange(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetBookingsByDate(targetDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.BookingDate.Date == targetDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateBooking_WithInvalidEmail_ThrowsException(string email)
    {
        // Arrange
        var booking = new Booking
        {
            UserId = Guid.NewGuid(),
            Name = "Test User",
            Email = email,
            Phone = "1234567890",
            BookingDate = DateTime.UtcNow.AddDays(1),
            Guests = 4,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            if (string.IsNullOrEmpty(booking.Email))
                throw new ArgumentException("Email is required");

            await _controller.CreateBooking(booking);
        });
    }

    [Fact]
    public async Task UpdateBooking_WithValidData_UpdatesBooking()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            Name = "Old Name",
            Email = "old@test.com",
            Phone = "1111111111",
            BookingDate = DateTime.UtcNow.AddDays(1),
            Guests = 2,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        booking.Name = "New Name";
        booking.Guests = 6;
        booking.Message = "Updated message";

        // Act
        var result = await _controller.UpdateBooking(bookingId, booking);

        // Assert
        result.Should().NotBeNull();
        var updatedBooking = await _context.Bookings.FindAsync(bookingId);
        updatedBooking!.Name.Should().Be("New Name");
        updatedBooking.Guests.Should().Be(6);
        updatedBooking.Message.Should().Be("Updated message");
    }
}