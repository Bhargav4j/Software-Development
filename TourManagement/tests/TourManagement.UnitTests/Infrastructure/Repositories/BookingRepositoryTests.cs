using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Infrastructure.Repositories;

public class BookingRepositoryTests
{
    private readonly Mock<ILogger<BookingRepository>> _mockLogger;

    public BookingRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<BookingRepository>>();
    }

    private TourManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourManagementDbContext(options);
    }

    [Fact]
    public void BookingRepository_Constructor_ShouldThrowWhenContextIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BookingRepository(null!, _mockLogger.Object));
    }

    [Fact]
    public void BookingRepository_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange
        using var context = CreateContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BookingRepository(context, null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnActiveBookings()
    {
        // Arrange
        using var context = CreateContext();
        context.Bookings.AddRange(
            new Booking { Email = "user1@test.com", FirstName = "User1", TourName = "Tour1", Place = "Place1", IsActive = true, CreatedBy = "Test" },
            new Booking { Email = "user2@test.com", FirstName = "User2", TourName = "Tour2", Place = "Place2", IsActive = true, CreatedBy = "Test" },
            new Booking { Email = "inactive@test.com", FirstName = "Inactive", TourName = "Tour3", Place = "Place3", IsActive = false, CreatedBy = "Test" }
        );
        await context.SaveChangesAsync();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.True(b.IsActive));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBooking_WhenBookingExists()
    {
        // Arrange
        using var context = CreateContext();
        var booking = new Booking { Email = "test@test.com", FirstName = "Test", TourName = "Tour", Place = "Place", IsActive = true, CreatedBy = "Test" };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBookingNotExists()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnBookingsForEmail()
    {
        // Arrange
        using var context = CreateContext();
        context.Bookings.AddRange(
            new Booking { Email = "test@test.com", FirstName = "Test1", TourName = "Tour1", Place = "Place1", IsActive = true, CreatedBy = "Test" },
            new Booking { Email = "test@test.com", FirstName = "Test2", TourName = "Tour2", Place = "Place2", IsActive = true, CreatedBy = "Test" },
            new Booking { Email = "other@test.com", FirstName = "Other", TourName = "Tour3", Place = "Place3", IsActive = true, CreatedBy = "Test" }
        );
        await context.SaveChangesAsync();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByEmailAsync("test@test.com");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal("test@test.com", b.Email));
    }

    [Fact]
    public async Task AddAsync_ShouldAddBooking()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new BookingRepository(context, _mockLogger.Object);
        var booking = new Booking { Email = "new@test.com", FirstName = "New", TourName = "Tour", Place = "Place", CreatedBy = "Test" };

        // Act
        var result = await repository.AddAsync(booking);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("new@test.com", result.Email);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBooking()
    {
        // Arrange
        using var context = CreateContext();
        var booking = new Booking { Email = "old@test.com", FirstName = "Old", TourName = "Old Tour", Place = "Place", IsActive = true, CreatedBy = "Test" };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        booking.TourName = "New Tour";
        await repository.UpdateAsync(booking);

        // Assert
        var updatedBooking = await context.Bookings.FindAsync(booking.Id);
        Assert.Equal("New Tour", updatedBooking!.TourName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkBookingAsInactive()
    {
        // Arrange
        using var context = CreateContext();
        var booking = new Booking { Email = "delete@test.com", FirstName = "Delete", TourName = "Tour", Place = "Place", IsActive = true, CreatedBy = "Test" };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        await repository.DeleteAsync(booking.Id);

        // Assert
        var deletedBooking = await context.Bookings.FindAsync(booking.Id);
        Assert.NotNull(deletedBooking);
        Assert.False(deletedBooking.IsActive);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenBookingExists()
    {
        // Arrange
        using var context = CreateContext();
        var booking = new Booking { Email = "test@test.com", FirstName = "Test", TourName = "Tour", Place = "Place", IsActive = true, CreatedBy = "Test" };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.ExistsAsync(booking.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenBookingNotExists()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.ExistsAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeTourRelation()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Test Tour", Place = "Place", IsActive = true, CreatedBy = "Test", Days = 5, Price = 500, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, Email = "test@test.com", FirstName = "Test", TourName = "Test Tour", Place = "Place", IsActive = true, CreatedBy = "Test" };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        var repository = new BookingRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        var firstBooking = result.First();
        Assert.NotNull(firstBooking.Tour);
    }
}
