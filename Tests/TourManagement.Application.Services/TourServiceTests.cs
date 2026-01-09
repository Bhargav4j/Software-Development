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
    public class TourServiceTests
    {
        private readonly Mock<ITourRepository> _mockTourRepository;
        private readonly Mock<ILogger<TourService>> _mockLogger;
        private readonly TourService _tourService;

        public TourServiceTests()
        {
            _mockTourRepository = new Mock<ITourRepository>();
            _mockLogger = new Mock<ILogger<TourService>>();
            _tourService = new TourService(_mockTourRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullTourRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TourService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TourService(_mockTourRepository.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var service = new TourService(_mockTourRepository.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetAllToursAsync_ReturnsAllTours()
        {
            // Arrange
            var expectedTours = new List<Tour>
            {
                new Tour { Id = 1, TourName = "Tour 1" },
                new Tour { Id = 2, TourName = "Tour 2" }
            };
            _mockTourRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTours);

            // Act
            var result = await _tourService.GetAllToursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTours, result);
            _mockTourRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllToursAsync_WithCancellationToken_PassesCancellationToken()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedTours = new List<Tour>();
            _mockTourRepository.Setup(r => r.GetAllAsync(cancellationToken))
                .ReturnsAsync(expectedTours);

            // Act
            await _tourService.GetAllToursAsync(cancellationToken);

            // Assert
            _mockTourRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAllToursAsync_WhenRepositoryThrowsException_RethrowsException()
        {
            // Arrange
            _mockTourRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _tourService.GetAllToursAsync());
        }

        [Fact]
        public async Task GetTourByIdAsync_WithValidId_ReturnsTour()
        {
            // Arrange
            var expectedTour = new Tour { Id = 1, TourName = "Test Tour" };
            _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTour);

            // Act
            var result = await _tourService.GetTourByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTour, result);
            Assert.Equal(1, result.Id);
            _mockTourRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTourByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            _mockTourRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tour?)null);

            // Act
            var result = await _tourService.GetTourByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTourByIdAsync_WhenRepositoryThrowsException_RethrowsException()
        {
            // Arrange
            _mockTourRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _tourService.GetTourByIdAsync(1));
        }

        [Fact]
        public async Task CreateTourAsync_WithNullTour_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _tourService.CreateTourAsync(null!));
        }

        [Fact]
        public async Task CreateTourAsync_WithValidTour_CreatesTourAndReturnsIt()
        {
            // Arrange
            var tour = new Tour { TourName = "New Tour", Place = "Paris" };
            var createdTour = new Tour { Id = 1, TourName = "New Tour", Place = "Paris" };
            _mockTourRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTour);

            // Act
            var result = await _tourService.CreateTourAsync(tour);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.True(tour.IsActive);
            Assert.NotEqual(default(DateTime), tour.CreatedDate);
            _mockTourRepository.Verify(r => r.AddAsync(tour, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateTourAsync_SetsCreatedDateToUtcNow()
        {
            // Arrange
            var tour = new Tour { TourName = "Test Tour" };
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
            _mockTourRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);

            // Act
            await _tourService.CreateTourAsync(tour);
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(tour.CreatedDate >= beforeCreation && tour.CreatedDate <= afterCreation);
        }

        [Fact]
        public async Task CreateTourAsync_SetsIsActiveToTrue()
        {
            // Arrange
            var tour = new Tour { TourName = "Test Tour", IsActive = false };
            _mockTourRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tour);

            // Act
            await _tourService.CreateTourAsync(tour);

            // Assert
            Assert.True(tour.IsActive);
        }

        [Fact]
        public async Task UpdateTourAsync_WithNullTour_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _tourService.UpdateTourAsync(null!));
        }

        [Fact]
        public async Task UpdateTourAsync_WithNonExistentTour_ThrowsInvalidOperationException()
        {
            // Arrange
            var tour = new Tour { Id = 999, TourName = "Non-existent" };
            _mockTourRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _tourService.UpdateTourAsync(tour));
        }

        [Fact]
        public async Task UpdateTourAsync_WithValidTour_UpdatesTour()
        {
            // Arrange
            var tour = new Tour { Id = 1, TourName = "Updated Tour" };
            _mockTourRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockTourRepository.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _tourService.UpdateTourAsync(tour);

            // Assert
            Assert.NotNull(tour.ModifiedDate);
            _mockTourRepository.Verify(r => r.UpdateAsync(tour, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTourAsync_SetsModifiedDateToUtcNow()
        {
            // Arrange
            var tour = new Tour { Id = 1, TourName = "Test Tour" };
            var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);
            _mockTourRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _tourService.UpdateTourAsync(tour);
            var afterUpdate = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.NotNull(tour.ModifiedDate);
            Assert.True(tour.ModifiedDate >= beforeUpdate && tour.ModifiedDate <= afterUpdate);
        }

        [Fact]
        public async Task DeleteTourAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockTourRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _tourService.DeleteTourAsync(999));
        }

        [Fact]
        public async Task DeleteTourAsync_WithValidId_DeletesTour()
        {
            // Arrange
            _mockTourRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockTourRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _tourService.DeleteTourAsync(1);

            // Assert
            _mockTourRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchToursAsync_WithSearchTerm_ReturnsMatchingTours()
        {
            // Arrange
            var searchTerm = "Europe";
            var expectedTours = new List<Tour>
            {
                new Tour { Id = 1, TourName = "European Tour" }
            };
            _mockTourRepository.Setup(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTours);

            // Act
            var result = await _tourService.SearchToursAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTours, result);
            _mockTourRepository.Verify(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchToursAsync_WithEmptySearchTerm_ReturnsAllMatchingTours()
        {
            // Arrange
            var searchTerm = "";
            var expectedTours = new List<Tour>();
            _mockTourRepository.Setup(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedTours);

            // Act
            var result = await _tourService.SearchToursAsync(searchTerm);

            // Assert
            Assert.NotNull(result);
            _mockTourRepository.Verify(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchToursAsync_WhenRepositoryThrowsException_RethrowsException()
        {
            // Arrange
            _mockTourRepository.Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Search error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _tourService.SearchToursAsync("test"));
        }
    }
}
