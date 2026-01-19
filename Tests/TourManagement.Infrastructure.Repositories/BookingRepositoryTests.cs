using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Repositories.Tests;

public class BookingRepositoryTests
{
    private readonly Mock<ILogger<BookingRepository>> _mockLogger;
    private readonly DbContextOptions<TourManagementDbContext> _options;
    private readonly TourManagementDbContext _context;
    private readonly BookingRepository _repository;

    public BookingRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<BookingRepository>>();
        _options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TourManagementDbContext(_options);
        _repository = new BookingRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookingRepository(null!, _mockLogger.Object));
        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookingRepository(_context, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange, Act & Assert
        Assert.NotNull(_repository);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveBookings()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, CreatedDate = DateTime.UtcNow },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-1) }
        };
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.True(b.IsActive));
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotReturnInactiveBookings()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, CreatedDate = DateTime.UtcNow },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = false, CreatedDate = DateTime.UtcNow }
        };
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsActive);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnBookingsOrderedByCreatedDateDescending()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-10) },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, CreatedDate = DateTime.UtcNow },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-5) }
        };
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var bookingsList = result.ToList();
        Assert.True(bookingsList[0].CreatedDate > bookingsList[1].CreatedDate);
        Assert.True(bookingsList[1].CreatedDate > bookingsList[2].CreatedDate);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnBooking()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveBooking_ShouldReturnNull()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = false };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(booking.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserBookings()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user1 = new User { Email = "user1@test.com", IsActive = true };
        var user2 = new User { Email = "user2@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new Booking { TourId = tour.Id, UserId = user1.Id, IsActive = true, BookingDate = DateTime.UtcNow },
            new Booking { TourId = tour.Id, UserId = user1.Id, IsActive = true, BookingDate = DateTime.UtcNow.AddDays(-1) },
            new Booking { TourId = tour.Id, UserId = user2.Id, IsActive = true, BookingDate = DateTime.UtcNow }
        };
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user1.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(user1.Id, b.UserId));
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnBookingsOrderedByBookingDateDescending()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow.AddDays(-10) },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow.AddDays(-5) }
        };
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        var bookingsList = result.ToList();
        Assert.True(bookingsList[0].BookingDate > bookingsList[1].BookingDate);
        Assert.True(bookingsList[1].BookingDate > bookingsList[2].BookingDate);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNoBookings_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByUserIdAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTourIdAsync_ShouldReturnTourBookings()
    {
        // Arrange
        var tour1 = new Tour { TourName = "Tour 1", IsActive = true };
        var tour2 = new Tour { TourName = "Tour 2", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddRangeAsync(tour1, tour2);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new Booking { TourId = tour1.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow },
            new Booking { TourId = tour1.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow.AddDays(-1) },
            new Booking { TourId = tour2.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow }
        };
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTourIdAsync(tour1.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(tour1.Id, b.TourId));
    }

    [Fact]
    public async Task GetByTourIdAsync_ShouldReturnBookingsOrderedByBookingDateDescending()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow.AddDays(-10) },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow },
            new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, BookingDate = DateTime.UtcNow.AddDays(-5) }
        };
        await _context.Bookings.AddRangeAsync(bookings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTourIdAsync(tour.Id);

        // Assert
        var bookingsList = result.ToList();
        Assert.True(bookingsList[0].BookingDate > bookingsList[1].BookingDate);
        Assert.True(bookingsList[1].BookingDate > bookingsList[2].BookingDate);
    }

    [Fact]
    public async Task GetByTourIdAsync_WithNoBookings_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByTourIdAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddBookingToDatabase()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking
        {
            TourId = tour.Id,
            UserId = user.Id,
            NumberOfPeople = 2,
            TotalAmount = 2000m,
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        var savedBooking = await _context.Bookings.FindAsync(result.Id);
        Assert.NotNull(savedBooking);
        Assert.Equal(2, savedBooking.NumberOfPeople);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnBookingWithGeneratedId()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };

        // Act
        var result = await _repository.AddAsync(booking);

        // Assert
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingBooking()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking
        {
            TourId = tour.Id,
            UserId = user.Id,
            NumberOfPeople = 2,
            Status = "Pending",
            IsActive = true
        };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
        _context.Entry(booking).State = EntityState.Detached;

        booking.NumberOfPeople = 4;
        booking.Status = "Confirmed";

        // Act
        await _repository.UpdateAsync(booking);

        // Assert
        var updatedBooking = await _context.Bookings.FindAsync(booking.Id);
        Assert.NotNull(updatedBooking);
        Assert.Equal(4, updatedBooking.NumberOfPeople);
        Assert.Equal("Confirmed", updatedBooking.Status);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteBooking()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
        var bookingId = booking.Id;

        // Act
        await _repository.DeleteAsync(bookingId);

        // Assert
        var deletedBooking = await _context.Bookings.FindAsync(bookingId);
        Assert.NotNull(deletedBooking);
        Assert.False(deletedBooking.IsActive);
        Assert.NotNull(deletedBooking.ModifiedDate);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrowException()
    {
        // Act & Assert
        await _repository.DeleteAsync(999);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetModifiedDate()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
        var bookingId = booking.Id;
        var beforeDelete = DateTime.UtcNow;

        // Act
        await _repository.DeleteAsync(bookingId);
        var afterDelete = DateTime.UtcNow;

        // Assert
        var deletedBooking = await _context.Bookings.FindAsync(bookingId);
        Assert.NotNull(deletedBooking!.ModifiedDate);
        Assert.True(deletedBooking.ModifiedDate >= beforeDelete && deletedBooking.ModifiedDate <= afterDelete);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingActiveBooking_ShouldReturnTrue()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(booking.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentBooking_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WithInactiveBooking_ShouldReturnFalse()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = false };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(booking.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeTourAndUserNavigationProperties()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true, CreatedDate = DateTime.UtcNow };
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var firstBooking = result.First();
        Assert.NotNull(firstBooking.Tour);
        Assert.NotNull(firstBooking.User);
        Assert.Equal("Test Tour", firstBooking.Tour.TourName);
        Assert.Equal("test@test.com", firstBooking.User.Email);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
