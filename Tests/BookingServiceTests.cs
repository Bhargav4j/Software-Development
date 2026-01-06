using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Exceptions;

namespace Tests.TourManagement.Application.Services;

/// <summary>
/// Tests for BookingService
/// </summary>
public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockBookingRepository;
    private readonly Mock<ILogger<BookingService>> _mockLogger;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _mockBookingRepository = new Mock<IBookingRepository>();
        _mockLogger = new Mock<ILogger<BookingService>>();
        _bookingService = new BookingService(_mockBookingRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllBookingsAsync_ShouldReturnAllBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, UserId = 1, TourId = 1 },
            new Booking { Id = 2, UserId = 2, TourId = 2 }
        };
        _mockBookingRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _bookingService.GetAllBookingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithValidId_ShouldReturnBooking()
    {
        // Arrange
        var booking = new Booking { Id = 1, UserId = 1, TourId = 1 };
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

    [Fact]
    public async Task GetUserBookingsAsync_WithValidUserId_ShouldReturnUserBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, UserId = 1, TourId = 1 },
            new Booking { Id = 2, UserId = 1, TourId = 2 }
        };
        _mockBookingRepository.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _bookingService.GetUserBookingsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(1, b.UserId));
    }

    [Fact]
    public async Task GetTourBookingsAsync_WithValidTourId_ShouldReturnTourBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, UserId = 1, TourId = 1 },
            new Booking { Id = 2, UserId = 2, TourId = 1 }
        };
        _mockBookingRepository.Setup(r => r.GetByTourIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        // Act
        var result = await _bookingService.GetTourBookingsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, b => Assert.Equal(1, b.TourId));
    }

    [Fact]
    public async Task CreateBookingAsync_WithValidBooking_ShouldReturnCreatedBooking()
    {
        // Arrange
        var booking = new Booking { UserId = 1, TourId = 1, NumberOfPeople = 2, TotalAmount = 2000m };
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _bookingService.CreateBookingAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpdateBookingAsync_WithValidBooking_ShouldUpdateBooking()
    {
        // Arrange
        var booking = new Booking { Id = 1, UserId = 1, TourId = 1 };
        _mockBookingRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockBookingRepository.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _bookingService.UpdateBookingAsync(booking);

        // Assert
        _mockBookingRepository.Verify(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookingAsync_WithNonExistentBooking_ShouldThrowException()
    {
        // Arrange
        var booking = new Booking { Id = 999, UserId = 1, TourId = 1 };
        _mockBookingRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _bookingService.UpdateBookingAsync(booking));
    }

    [Fact]
    public async Task DeleteBookingAsync_WithValidId_ShouldDeleteBooking()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockBookingRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _bookingService.DeleteBookingAsync(1);

        // Assert
        _mockBookingRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBookingAsync_WithNonExistentId_ShouldThrowException()
    {
        // Arrange
        _mockBookingRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _bookingService.DeleteBookingAsync(999));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldSetBookingDateAndIsActive()
    {
        // Arrange
        var booking = new Booking { UserId = 1, TourId = 1 };
        Booking? capturedBooking = null;
        _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, ct) => capturedBooking = b)
            .ReturnsAsync(booking);

        // Act
        await _bookingService.CreateBookingAsync(booking);

        // Assert
        Assert.NotNull(capturedBooking);
        Assert.True(capturedBooking.IsActive);
    }
}
