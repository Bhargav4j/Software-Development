using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.UnitTests.Application.Services;

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
    public void TourService_Constructor_ShouldThrowWhenRepositoryIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TourService(null!, _mockLogger.Object));
    }

    [Fact]
    public void TourService_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TourService(_mockRepository.Object, null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Tour 1" },
            new Tour { Id = 2, TourName = "Tour 2" }
        };
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tours);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyListWhenNoTours()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Tour>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTour_WhenTourExists()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tour);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Tour", result.TourName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTourNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Tour?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTour()
    {
        // Arrange
        var tour = new Tour { TourName = "New Tour", Place = "Paris" };
        var createdTour = new Tour { Id = 1, TourName = "New Tour", Place = "Paris", IsActive = true };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdTour);

        // Act
        var result = await _service.CreateAsync(tour);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.True(result.IsActive);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetCreatedDateAndIsActive()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour" };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tour t, CancellationToken ct) => t);

        // Act
        var result = await _service.CreateAsync(tour);

        // Assert
        Assert.True(result.IsActive);
        Assert.NotEqual(default(DateTime), result.CreatedDate);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTour_WhenTourExists()
    {
        // Arrange
        var existingTour = new Tour { Id = 1, TourName = "Old Name", Place = "Old Place" };
        var updatedTour = new Tour { TourName = "New Name", Place = "New Place", Days = 5 };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTour);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(1, updatedTour);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenTourNotExists()
    {
        // Arrange
        var tour = new Tour { TourName = "Test" };
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Tour?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(999, tour));
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetModifiedDate()
    {
        // Arrange
        var existingTour = new Tour { Id = 1, TourName = "Old" };
        var updatedTour = new Tour { TourName = "New" };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingTour);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .Callback<Tour, CancellationToken>((t, ct) =>
            {
                Assert.NotNull(t.ModifiedDate);
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(1, updatedTour);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTour_WhenTourExists()
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
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenTourNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(999));
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Paris Tour" },
            new Tour { Id = 2, TourName = "Paris Night Tour" }
        };
        _mockRepository.Setup(r => r.SearchAsync("Paris", It.IsAny<CancellationToken>())).ReturnsAsync(tours);

        // Act
        var result = await _service.SearchAsync("Paris");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenNoMatches()
    {
        // Arrange
        _mockRepository.Setup(r => r.SearchAsync("NonExistent", It.IsAny<CancellationToken>())).ReturnsAsync(new List<Tour>());

        // Act
        var result = await _service.SearchAsync("NonExistent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
