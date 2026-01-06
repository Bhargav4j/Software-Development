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
    public class TourServiceTests
    {
        private readonly Mock<ITourRepository> _mockRepository;
        private readonly Mock<ILogger<TourService>> _mockLogger;
        private readonly TourService _service;

        public TourServiceTests()
        {
            _mockRepository = new Mock<ITourRepository>();
            _mockLogger = new Mock<ILogger<TourService>>();
            _service = new TourService(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TourService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TourService(_mockRepository.Object, null!));
        }

        [Fact]
        public async Task GetAllToursAsync_ShouldReturnTours()
        {
            // Arrange
            var tours = new List<Tour>
            {
                new Tour { Id = 1, TourName = "Tour 1" },
                new Tour { Id = 2, TourName = "Tour 2" }
            };
            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(tours);

            // Act
            var result = await _service.GetAllToursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<Tour>)result).Count);
            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTourByIdAsync_ShouldReturnTour()
        {
            // Arrange
            var tour = new Tour { Id = 1, TourName = "Test Tour" };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);

            // Act
            var result = await _service.GetTourByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Tour", result.TourName);
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTourByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tour?)null);

            // Act
            var result = await _service.GetTourByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateTourAsync_ShouldCreateTour()
        {
            // Arrange
            var tour = new Tour { TourName = "New Tour", Place = "Paris" };
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);

            // Act
            var result = await _service.CreateTourAsync(tour);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsActive);
            Assert.Equal("System", result.CreatedBy);
            Assert.NotEqual(default(DateTime), result.CreatedDate);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTourAsync_WithExistingTour_ShouldUpdateTour()
        {
            // Arrange
            var existingTour = new Tour { Id = 1, TourName = "Old Tour" };
            var updatedTour = new Tour { TourName = "Updated Tour", Place = "London" };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTour);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateTourAsync(1, updatedTour);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTourAsync_WithNonExistentTour_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var updatedTour = new Tour { TourName = "Updated Tour" };
            _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tour?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTourAsync(999, updatedTour));
            _mockRepository.Verify(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTourAsync_WithExistingTour_ShouldDeleteTour()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteTourAsync(1);

            // Assert
            _mockRepository.Verify(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTourAsync_WithNonExistentTour_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteTourAsync(999));
            _mockRepository.Verify(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchToursAsync_ShouldReturnMatchingTours()
        {
            // Arrange
            var tours = new List<Tour>
            {
                new Tour { Id = 1, TourName = "Paris Tour" },
                new Tour { Id = 2, TourName = "Paris Adventure" }
            };
            _mockRepository.Setup(r => r.SearchAsync("Paris", It.IsAny<CancellationToken>()))
                .ReturnsAsync(tours);

            // Act
            var result = await _service.SearchToursAsync("Paris");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<Tour>)result).Count);
            _mockRepository.Verify(r => r.SearchAsync("Paris", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchToursAsync_WithEmptyString_ShouldCallRepository()
        {
            // Arrange
            _mockRepository.Setup(r => r.SearchAsync("", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Tour>());

            // Act
            var result = await _service.SearchToursAsync("");

            // Assert
            Assert.NotNull(result);
            _mockRepository.Verify(r => r.SearchAsync("", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
