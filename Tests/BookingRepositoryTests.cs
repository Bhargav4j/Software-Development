using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Domain.Entities;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;

namespace Tests.TourManagement.Infrastructure.Repositories;

/// <summary>
/// Tests for BookingRepository
/// </summary>
public class BookingRepositoryTests
{
    private readonly DbContextOptions<TourManagementDbContext> _options;
    private readonly Mock<ILogger<BookingRepository>> _mockLogger;

    public BookingRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockLogger = new Mock<ILogger<BookingRepository>>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveBookings()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);

        context.Bookings.AddRange(
            new Booking { Id = 1, UserId = 1, TourId = 1, IsActive = true },
            new Booking { Id = 2, UserId = 1, TourId = 1, IsActive = false },
            new Booking { Id = 3, UserId = 1, TourId = 1, IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.True(b.IsActive));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnBooking()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);

        var booking = new Booking { Id = 1, UserId = 1, TourId = 1, IsActive = true };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnUserBookings()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);

        context.Bookings.AddRange(
            new Booking { Id = 1, UserId = 1, TourId = 1, IsActive = true },
            new Booking { Id = 2, UserId = 1, TourId = 1, IsActive = true },
            new Booking { Id = 3, UserId = 2, TourId = 1, IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByUserIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(1, b.UserId));
    }

    [Fact]
    public async Task GetByTourIdAsync_WithValidTourId_ShouldReturnTourBookings()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);

        context.Bookings.AddRange(
            new Booking { Id = 1, UserId = 1, TourId = 1, IsActive = true },
            new Booking { Id = 2, UserId = 1, TourId = 1, IsActive = true },
            new Booking { Id = 3, UserId = 1, TourId = 2, IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByTourIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(1, b.TourId));
    }

    [Fact]
    public async Task AddAsync_ShouldAddBookingToDatabase()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        var booking = new Booking { UserId = 1, TourId = 1, IsActive = true };

        // Act
        var result = await repository.AddAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBooking()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);

        var booking = new Booking { UserId = 1, TourId = 1, Status = "Pending", IsActive = true };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Act
        booking.Status = "Confirmed";
        await repository.UpdateAsync(booking);

        // Assert
        var updatedBooking = await context.Bookings.FindAsync(booking.Id);
        Assert.Equal("Confirmed", updatedBooking!.Status);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetBookingAsInactive()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);

        var booking = new Booking { UserId = 1, TourId = 1, IsActive = true };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(booking.Id);

        // Assert
        var deletedBooking = await context.Bookings.FindAsync(booking.Id);
        Assert.False(deletedBooking!.IsActive);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new BookingRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com" };
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        context.Users.Add(user);
        context.Tours.Add(tour);

        var booking = new Booking { UserId = 1, TourId = 1, IsActive = true };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.ExistsAsync(booking.Id);

        // Assert
        Assert.True(result);
    }
}
