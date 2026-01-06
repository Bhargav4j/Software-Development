using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using Xunit;

namespace TourManagement.Application.Services.Tests
{
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
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BookingService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BookingService(_mockRepository.Object, null!));
        }

        [Fact]
        public async Task GetAllBookingsAsync_ShouldReturnBookings()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, UserId = 1, TourId = 1 },
                new Booking { Id = 2, UserId = 2, TourId = 2 }
            };
            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _service.GetAllBookingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<Booking>)result).Count);
            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetBookingByIdAsync_ShouldReturnBooking()
        {
            // Arrange
            var booking = new Booking { Id = 1, UserId = 1, TourId = 1 };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _service.GetBookingByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetBookingsByUserIdAsync_ShouldReturnBookings()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, UserId = 1, TourId = 1 },
                new Booking { Id = 2, UserId = 1, TourId = 2 }
            };
            _mockRepository.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _service.GetBookingsByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<Booking>)result).Count);
            _mockRepository.Verify(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetBookingsByTourIdAsync_ShouldReturnBookings()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { Id = 1, UserId = 1, TourId = 1 },
                new Booking { Id = 2, UserId = 2, TourId = 1 }
            };
            _mockRepository.Setup(r => r.GetByTourIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _service.GetBookingsByTourIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<Booking>)result).Count);
            _mockRepository.Verify(r => r.GetByTourIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldCreateBooking()
        {
            // Arrange
            var booking = new Booking { UserId = 1, TourId = 1, NumberOfPeople = 2 };
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _service.CreateBookingAsync(booking);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pending", result.Status);
            Assert.True(result.IsActive);
            Assert.Equal("System", result.CreatedBy);
            Assert.NotEqual(default(DateTime), result.CreatedDate);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookingAsync_WithExistingBooking_ShouldUpdateBooking()
        {
            // Arrange
            var existingBooking = new Booking { Id = 1, UserId = 1, TourId = 1, Status = "Pending" };
            var updatedBooking = new Booking { Status = "Confirmed", NumberOfPeople = 4 };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBooking);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateBookingAsync(1, updatedBooking);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookingAsync_WithNonExistentBooking_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var updatedBooking = new Booking { Status = "Confirmed" };
            _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateBookingAsync(999, updatedBooking));
        }

        [Fact]
        public async Task DeleteBookingAsync_WithExistingBooking_ShouldDeleteBooking()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteBookingAsync(1);

            // Assert
            _mockRepository.Verify(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteBookingAsync_WithNonExistentBooking_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteBookingAsync(999));
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            // Act
            var result = await _service.GetBookingByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBookingsByUserIdAsync_WithNoBookings_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByUserIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _service.GetBookingsByUserIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBookingsByTourIdAsync_WithNoBookings_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByTourIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _service.GetBookingsByTourIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
