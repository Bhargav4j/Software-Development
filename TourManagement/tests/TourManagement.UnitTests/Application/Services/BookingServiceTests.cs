using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.UnitTests.Application.Services;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<ILogger<BookingService>> _mockLogger;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _mockRepository = new Mock<IBookingRepository>();
        _mockLogger = new Mock<ILogger<BookingService>>();
        _service = new BookingService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public void BookingService_Constructor_ShouldThrowWhenRepositoryIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BookingService(null!, _mockLogger.Object));
    }

    [Fact]
    public void BookingService_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BookingService(_mockRepository.Object, null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBookings()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, Email = "user1@test.com" },
            new Booking { Id = 2, Email = "user2@test.com" }
        };
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyListWhenNoBookings()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Booking>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBooking_WhenBookingExists()
    {
        // Arrange
        var booking = new Booking { Id = 1, Email = "test@test.com", TourName = "Test Tour" };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBookingNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnBookingsForEmail()
    {
        // Arrange
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, Email = "test@test.com" },
            new Booking { Id = 2, Email = "test@test.com" }
        };
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetByEmailAsync("test@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnEmpty_WhenNoBookingsForEmail()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(new List<Booking>());

        // Act
        var result = await _service.GetByEmailAsync("test@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBooking()
    {
        // Arrange
        var booking = new Booking { Email = "test@test.com", TourName = "Test Tour" };
        var createdBooking = new Booking { Id = 1, Email = "test@test.com", TourName = "Test Tour", IsActive = true };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdBooking);

        // Act
        var result = await _service.CreateAsync(booking);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetBookingDateAndCreatedDate()
    {
        // Arrange
        var booking = new Booking { Email = "test@test.com" };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) => b);

        // Act
        var result = await _service.CreateAsync(booking);

        // Assert
        Assert.NotEqual(default(DateTime), result.BookingDate);
        Assert.NotEqual(default(DateTime), result.CreatedDate);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBooking_WhenBookingExists()
    {
        // Arrange
        var existingBooking = new Booking { Id = 1, Email = "old@test.com", TourName = "Old Tour" };
        var updatedBooking = new Booking { Email = "new@test.com", TourName = "New Tour" };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingBooking);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(1, updatedBooking);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenBookingNotExists()
    {
        // Arrange
        var booking = new Booking { Email = "test@test.com" };
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(999, booking));
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetModifiedDate()
    {
        // Arrange
        var existingBooking = new Booking { Id = 1, Email = "test@test.com" };
        var updatedBooking = new Booking { Email = "updated@test.com" };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingBooking);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, ct) =>
            {
                Assert.NotNull(b.ModifiedDate);
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(1, updatedBooking);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteBooking_WhenBookingExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenBookingNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(999));
    }
}
