using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.Application.Services.Tests
{
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
        public void Constructor_WithNullBookingRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BookingService(null!, _mockTourRepository.Object, _mockUserRepository.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullTourRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BookingService(_mockBookingRepository.Object, null!, _mockUserRepository.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullUserRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BookingService(_mockBookingRepository.Object, _mockTourRepository.Object, null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BookingService(_mockBookingRepository.Object, _mockTourRepository.Object, _mockUserRepository.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var service = new BookingService(
                _mockBookingRepository.Object,
                _mockTourRepository.Object,
                _mockUserRepository.Object,
                _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetAllBookingsAsync_ReturnsAllBookings()
        {
            // Arrange
            var expectedBookings = new List<Booking>
            {
                new Booking { Id = 1, TourId = 1, UserId = 1 },
                new Booking { Id = 2, TourId = 2, UserId = 2 }
            };
            _mockBookingRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedBookings);

            // Act
            var result = await _bookingService.GetAllBookingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBookings, result);
            _mockBookingRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllBookingsAsync_WhenRepositoryThrowsException_RethrowsException()
        {
            // Arrange
            _mockBookingRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _bookingService.GetAllBookingsAsync());
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithValidId_ReturnsBooking()
        {
            // Arrange
            var expectedBooking = new Booking { Id = 1, TourId = 1, UserId = 1 };
            _mockBookingRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedBooking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBooking, result);
            Assert.Equal(1, result.Id);
            _mockBookingRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnsNull()
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
        public async Task GetUserBookingsAsync_WithValidUserId_ReturnsUserBookings()
        {
            // Arrange
            var userId = 1;
            var expectedBookings = new List<Booking>
            {
                new Booking { Id = 1, TourId = 1, UserId = userId },
                new Booking { Id = 2, TourId = 2, UserId = userId }
            };
            _mockBookingRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedBookings);

            // Act
            var result = await _bookingService.GetUserBookingsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBookings, result);
            _mockBookingRepository.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserBookingsAsync_WhenRepositoryThrowsException_RethrowsException()
        {
            // Arrange
            _mockBookingRepository.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _bookingService.GetUserBookingsAsync(1));
        }

        [Fact]
        public async Task GetTourBookingsAsync_WithValidTourId_ReturnsTourBookings()
        {
            // Arrange
            var tourId = 1;
            var expectedBookings = new List<Booking>
            {
                new Booking { Id = 1, TourId = tourId, UserId = 1 },
                new Booking { Id = 2, TourId = tourId, UserId = 2 }
            };
            _mockBookingRepository.Setup(r => r.GetByTourIdAsync(tourId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedBookings);

            // Act
            var result = await _bookingService.GetTourBookingsAsync(tourId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBookings, result);
            _mockBookingRepository.Verify(r => r.GetByTourIdAsync(tourId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_WithNullBooking_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _bookingService.CreateBookingAsync(null!));
        }

        [Fact]
        public async Task CreateBookingAsync_WithNonExistentTour_ThrowsInvalidOperationException()
        {
            // Arrange
            var booking = new Booking { TourId = 999, UserId = 1, NumberOfPersons = 2 };
            _mockTourRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tour?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bookingService.CreateBookingAsync(booking));
        }

        [Fact]
        public async Task CreateBookingAsync_WithNonExistentUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var booking = new Booking { TourId = 1, UserId = 999, NumberOfPersons = 2 };
            var tour = new Tour { Id = 1, Price = 1000 };
            _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);
            _mockUserRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bookingService.CreateBookingAsync(booking));
        }

        [Fact]
        public async Task CreateBookingAsync_WithValidData_CreatesBookingAndReturnsIt()
        {
            // Arrange
            var booking = new Booking { TourId = 1, UserId = 1, NumberOfPersons = 3 };
            var tour = new Tour { Id = 1, Price = 1000 };
            var user = new User { Id = 1, Email = "test@example.com" };
            var createdBooking = new Booking { Id = 1, TourId = 1, UserId = 1, NumberOfPersons = 3 };

            _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);
            _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdBooking);

            // Act
            var result = await _bookingService.CreateBookingAsync(booking);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.True(booking.IsActive);
            Assert.NotEqual(default(DateTime), booking.CreatedDate);
            Assert.NotEqual(default(DateTime), booking.BookingDate);
            Assert.Equal(3000, booking.TotalAmount); // 1000 * 3
            _mockBookingRepository.Verify(r => r.AddAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_CalculatesTotalAmountCorrectly()
        {
            // Arrange
            var booking = new Booking { TourId = 1, UserId = 1, NumberOfPersons = 5 };
            var tour = new Tour { Id = 1, Price = 250.50m };
            var user = new User { Id = 1, Email = "test@example.com" };

            _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);
            _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            await _bookingService.CreateBookingAsync(booking);

            // Assert
            Assert.Equal(1252.50m, booking.TotalAmount); // 250.50 * 5
        }

        [Fact]
        public async Task CreateBookingAsync_SetsCreatedDateToUtcNow()
        {
            // Arrange
            var booking = new Booking { TourId = 1, UserId = 1, NumberOfPersons = 2 };
            var tour = new Tour { Id = 1, Price = 1000 };
            var user = new User { Id = 1, Email = "test@example.com" };
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

            _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);
            _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            await _bookingService.CreateBookingAsync(booking);
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(booking.CreatedDate >= beforeCreation && booking.CreatedDate <= afterCreation);
        }

        [Fact]
        public async Task CreateBookingAsync_SetsBookingDateToUtcNow()
        {
            // Arrange
            var booking = new Booking { TourId = 1, UserId = 1, NumberOfPersons = 2 };
            var tour = new Tour { Id = 1, Price = 1000 };
            var user = new User { Id = 1, Email = "test@example.com" };
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

            _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);
            _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            await _bookingService.CreateBookingAsync(booking);
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(booking.BookingDate >= beforeCreation && booking.BookingDate <= afterCreation);
        }

        [Fact]
        public async Task UpdateBookingAsync_WithNullBooking_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _bookingService.UpdateBookingAsync(null!));
        }

        [Fact]
        public async Task UpdateBookingAsync_WithNonExistentBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var booking = new Booking { Id = 999, TourId = 1, UserId = 1 };
            _mockBookingRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bookingService.UpdateBookingAsync(booking));
        }

        [Fact]
        public async Task UpdateBookingAsync_WithValidBooking_UpdatesBooking()
        {
            // Arrange
            var booking = new Booking { Id = 1, TourId = 1, UserId = 1 };
            _mockBookingRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockBookingRepository.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _bookingService.UpdateBookingAsync(booking);

            // Assert
            Assert.NotNull(booking.ModifiedDate);
            _mockBookingRepository.Verify(r => r.UpdateAsync(booking, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookingAsync_SetsModifiedDateToUtcNow()
        {
            // Arrange
            var booking = new Booking { Id = 1, TourId = 1, UserId = 1 };
            var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);
            _mockBookingRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _bookingService.UpdateBookingAsync(booking);
            var afterUpdate = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.NotNull(booking.ModifiedDate);
            Assert.True(booking.ModifiedDate >= beforeUpdate && booking.ModifiedDate <= afterUpdate);
        }

        [Fact]
        public async Task DeleteBookingAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockBookingRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bookingService.DeleteBookingAsync(999));
        }

        [Fact]
        public async Task DeleteBookingAsync_WithValidId_DeletesBooking()
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
    }
}
