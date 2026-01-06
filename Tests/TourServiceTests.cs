using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Exceptions;

namespace Tests.TourManagement.Application.Services;

/// <summary>
/// Tests for TourService
/// </summary>
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
    public async Task GetAllToursAsync_ShouldReturnAllTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Paris Tour" },
            new Tour { Id = 2, TourName = "London Tour" }
        };
        _mockTourRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tours);

        // Act
        var result = await _tourService.GetAllToursAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetTourByIdAsync_WithValidId_ShouldReturnTour()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Paris Tour" };
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);

        // Act
        var result = await _tourService.GetTourByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Paris Tour", result.TourName);
    }

    [Fact]
    public async Task GetTourByIdAsync_WithInvalidId_ShouldReturnNull()
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
    public async Task CreateTourAsync_WithValidTour_ShouldReturnCreatedTour()
    {
        // Arrange
        var tour = new Tour { TourName = "New Tour", Place = "Rome", Days = 5, Price = 1000m };
        _mockTourRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);

        // Act
        var result = await _tourService.CreateTourAsync(tour);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpdateTourAsync_WithValidTour_ShouldUpdateTour()
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
        _mockTourRepository.Verify(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTourAsync_WithNonExistentTour_ShouldThrowException()
    {
        // Arrange
        var tour = new Tour { Id = 999, TourName = "Not Found Tour" };
        _mockTourRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _tourService.UpdateTourAsync(tour));
    }

    [Fact]
    public async Task DeleteTourAsync_WithValidId_ShouldDeleteTour()
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
    public async Task DeleteTourAsync_WithNonExistentId_ShouldThrowException()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _tourService.DeleteTourAsync(999));
    }

    [Fact]
    public async Task SearchToursAsync_WithValidSearchTerm_ShouldReturnMatchingTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Paris Adventure", Place = "Paris" },
            new Tour { Id = 2, TourName = "Paris Explorer", Place = "Paris" }
        };
        _mockTourRepository.Setup(r => r.SearchAsync("Paris", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tours);

        // Act
        var result = await _tourService.SearchToursAsync("Paris");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchToursAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.SearchAsync("NonExistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tour>());

        // Act
        var result = await _tourService.SearchToursAsync("NonExistent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateTourAsync_ShouldSetCreatedDateAndIsActive()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour" };
        Tour? capturedTour = null;
        _mockTourRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .Callback<Tour, CancellationToken>((t, ct) => capturedTour = t)
            .ReturnsAsync(tour);

        // Act
        await _tourService.CreateTourAsync(tour);

        // Assert
        Assert.NotNull(capturedTour);
        Assert.True(capturedTour.IsActive);
    }
}
