using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.Application.Services.Tests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockBookingRepository;
    private readonly Mock<ITourRepository> _mockTourRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<BookingService>> _mockLogger;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _mockBookingRepository = new Mock<IBookingRepository>();
        _mockTourRepository = new Mock<ITourRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<BookingService>>();
        _bookingService = new BookingService(
            _mockBookingRepository.Object,
            _mockTourRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullBookingRepository_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookingService(null!, _mockTourRepository.Object, _mockUserRepository.Object, _mockLogger.Object));
        Assert.Equal("bookingRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullTourRepository_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookingService(_mockBookingRepository.Object, null!, _mockUserRepository.Object, _mockLogger.Object));
        Assert.Equal("tourRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookingService(_mockBookingRepository.Object, _mockTourRepository.Object, null!, _mockLogger.Object));
        Assert.Equal("userRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookingService(_mockBookingRepository.Object, _mockTourRepository.Object, _mockUserRepository.Object, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange, Act & Assert
        Assert.NotNull(_bookingService);
    }

    [Fact]
    public async Task GetAllBookingsAsync_ShouldReturnAllBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, TourId = 1, UserId = 1 },
            new Booking { Id = 2, TourId = 2, UserId = 2 }
        };
        _mockBookingRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _bookingService.GetAllBookingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockBookingRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllBookingsAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _bookingService.GetAllBookingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithValidId_ShouldReturnBooking()
    {
        // Arrange
        var booking = new Booking { Id = 1, TourId = 1, UserId = 1 };
        _mockBookingRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _bookingService.GetBookingByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _bookingService.GetBookingByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task GetBookingByIdAsync_WithDifferentIds_ShouldCallRepository(int id)
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Booking { Id = id });

        // Act
        await _bookingService.GetBookingByIdAsync(id);

        // Assert
        _mockBookingRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserBookingsAsync_ShouldReturnUserBookings()
    {
        // Arrange
        var userId = 1;
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, TourId = 1, UserId = userId },
            new Booking { Id = 2, TourId = 2, UserId = userId }
        };
        _mockBookingRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _bookingService.GetUserBookingsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(userId, b.UserId));
    }

    [Fact]
    public async Task GetUserBookingsAsync_WithNoBookings_ShouldReturnEmptyList()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _bookingService.GetUserBookingsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task GetUserBookingsAsync_WithDifferentUserIds_ShouldCallRepository(int userId)
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        await _bookingService.GetUserBookingsAsync(userId);

        // Assert
        _mockBookingRepository.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTourBookingsAsync_ShouldReturnTourBookings()
    {
        // Arrange
        var tourId = 1;
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, TourId = tourId, UserId = 1 },
            new Booking { Id = 2, TourId = tourId, UserId = 2 }
        };
        _mockBookingRepository.Setup(r => r.GetByTourIdAsync(tourId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _bookingService.GetTourBookingsAsync(tourId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(tourId, b.TourId));
    }

    [Fact]
    public async Task GetTourBookingsAsync_WithNoBookings_ShouldReturnEmptyList()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByTourIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        var result = await _bookingService.GetTourBookingsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task GetTourBookingsAsync_WithDifferentTourIds_ShouldCallRepository(int tourId)
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByTourIdAsync(tourId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>());

        // Act
        await _bookingService.GetTourBookingsAsync(tourId);

        // Assert
        _mockBookingRepository.Verify(r => r.GetByTourIdAsync(tourId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_WithValidBooking_ShouldCreateBooking()
    {
        // Arrange
        var tour = new Tour { Id = 1, Price = 1000m };
        var user = new User { Id = 1 };
        var booking = new Booking { TourId = 1, UserId = 1, NumberOfPeople = 2 };

        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _bookingService.CreateBookingAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2000m, booking.TotalAmount);
        Assert.Equal("Confirmed", booking.Status);
        Assert.True(booking.IsActive);
        Assert.NotEqual(default(DateTime), booking.CreatedDate);
        _mockBookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_WithNonExistentTour_ShouldThrowNotFoundException()
    {
        // Arrange
        var booking = new Booking { TourId = 999, UserId = 1 };
        _mockTourRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tour?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.CreateBookingAsync(booking));
    }

    [Fact]
    public async Task CreateBookingAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var tour = new Tour { Id = 1 };
        var booking = new Booking { TourId = 1, UserId = 999 };
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);
        _mockUserRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.CreateBookingAsync(booking));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldCalculateTotalAmountCorrectly()
    {
        // Arrange
        var tour = new Tour { Id = 1, Price = 500m };
        var user = new User { Id = 1 };
        var booking = new Booking { TourId = 1, UserId = 1, NumberOfPeople = 3 };

        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        await _bookingService.CreateBookingAsync(booking);

        // Assert
        Assert.Equal(1500m, booking.TotalAmount);
    }

    [Theory]
    [InlineData(1, 100, 100)]
    [InlineData(2, 100, 200)]
    [InlineData(5, 250, 1250)]
    [InlineData(10, 99.99, 999.90)]
    public async Task CreateBookingAsync_WithDifferentQuantities_ShouldCalculateCorrectly(int numberOfPeople, double price, double expectedTotal)
    {
        // Arrange
        var tour = new Tour { Id = 1, Price = (decimal)price };
        var user = new User { Id = 1 };
        var booking = new Booking { TourId = 1, UserId = 1, NumberOfPeople = numberOfPeople };

        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        await _bookingService.CreateBookingAsync(booking);

        // Assert
        Assert.Equal((decimal)expectedTotal, booking.TotalAmount);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldSetStatusToConfirmed()
    {
        // Arrange
        var tour = new Tour { Id = 1, Price = 1000m };
        var user = new User { Id = 1 };
        var booking = new Booking { TourId = 1, UserId = 1, NumberOfPeople = 1 };

        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        await _bookingService.CreateBookingAsync(booking);

        // Assert
        Assert.Equal("Confirmed", booking.Status);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldSetCreatedDateToUtcNow()
    {
        // Arrange
        var tour = new Tour { Id = 1, Price = 1000m };
        var user = new User { Id = 1 };
        var booking = new Booking { TourId = 1, UserId = 1, NumberOfPeople = 1 };
        var beforeCreate = DateTime.UtcNow;

        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        await _bookingService.CreateBookingAsync(booking);
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(booking.CreatedDate >= beforeCreate && booking.CreatedDate <= afterCreate);
    }

    [Fact]
    public async Task UpdateBookingAsync_WithExistingBooking_ShouldUpdateBooking()
    {
        // Arrange
        var booking = new Booking { Id = 1, TourId = 1, UserId = 1 };
        _mockBookingRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Booking { Id = 1 });
        _mockBookingRepository.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _bookingService.UpdateBookingAsync(booking);

        // Assert
        Assert.NotEqual(default(DateTime), booking.ModifiedDate);
        _mockBookingRepository.Verify(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookingAsync_WithNonExistingBooking_ShouldThrowNotFoundException()
    {
        // Arrange
        var booking = new Booking { Id = 999 };
        _mockBookingRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.UpdateBookingAsync(booking));
    }

    [Fact]
    public async Task UpdateBookingAsync_ShouldSetModifiedDateToUtcNow()
    {
        // Arrange
        var booking = new Booking { Id = 1 };
        var beforeUpdate = DateTime.UtcNow;
        _mockBookingRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Booking { Id = 1 });
        _mockBookingRepository.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _bookingService.UpdateBookingAsync(booking);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(booking.ModifiedDate);
        Assert.True(booking.ModifiedDate >= beforeUpdate && booking.ModifiedDate <= afterUpdate);
    }

    [Fact]
    public async Task DeleteBookingAsync_WithExistingBooking_ShouldDeleteBooking()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Booking { Id = 1 });
        _mockBookingRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _bookingService.DeleteBookingAsync(1);

        // Assert
        _mockBookingRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBookingAsync_WithNonExistingBooking_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _bookingService.DeleteBookingAsync(999));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(999)]
    public async Task DeleteBookingAsync_WithDifferentIds_ShouldCallRepository(int id)
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Booking { Id = id });
        _mockBookingRepository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _bookingService.DeleteBookingAsync(id);

        // Assert
        _mockBookingRepository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllBookingsAsync_WhenRepositoryThrowsException_ShouldRethrowException()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _bookingService.GetAllBookingsAsync());
    }

    [Fact]
    public async Task CreateBookingAsync_WhenRepositoryThrowsException_ShouldRethrowException()
    {
        // Arrange
        var tour = new Tour { Id = 1, Price = 1000m };
        var user = new User { Id = 1 };
        var booking = new Booking { TourId = 1, UserId = 1, NumberOfPeople = 1 };

        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _bookingService.CreateBookingAsync(booking));
    }
}
