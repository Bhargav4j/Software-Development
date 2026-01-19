using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.Application.Services.Tests;

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
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TourService(null!, _mockLogger.Object));
        Assert.Equal("tourRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TourService(_mockTourRepository.Object, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange, Act & Assert
        Assert.NotNull(_tourService);
    }

    [Fact]
    public async Task GetAllToursAsync_ShouldReturnAllTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Tour 1" },
            new Tour { Id = 2, TourName = "Tour 2" }
        };
        _mockTourRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tours);

        // Act
        var result = await _tourService.GetAllToursAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockTourRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllToursAsync_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tour>());

        // Act
        var result = await _tourService.GetAllToursAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllToursAsync_WithCancellationToken_ShouldPassCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        _mockTourRepository.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(new List<Tour>());

        // Act
        await _tourService.GetAllToursAsync(cancellationToken);

        // Assert
        _mockTourRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetTourByIdAsync_WithValidId_ShouldReturnTour()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);

        // Act
        var result = await _tourService.GetTourByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Tour", result.TourName);
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

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task GetTourByIdAsync_WithDifferentIds_ShouldCallRepository(int id)
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tour { Id = id });

        // Act
        await _tourService.GetTourByIdAsync(id);

        // Assert
        _mockTourRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTourAsync_WithValidTour_ShouldCreateTour()
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
        _mockTourRepository.Verify(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTourAsync_ShouldSetCreatedDateToUtcNow()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour" };
        var beforeCreate = DateTime.UtcNow;
        _mockTourRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tour);

        // Act
        await _tourService.CreateTourAsync(tour);
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(tour.CreatedDate >= beforeCreate && tour.CreatedDate <= afterCreate);
    }

    [Fact]
    public async Task CreateTourAsync_ShouldSetIsActiveToTrue()
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
    public async Task UpdateTourAsync_WithExistingTour_ShouldUpdateTour()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Updated Tour" };
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tour { Id = 1 });
        _mockTourRepository.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _tourService.UpdateTourAsync(tour);

        // Assert
        Assert.NotEqual(default(DateTime), tour.ModifiedDate);
        _mockTourRepository.Verify(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTourAsync_WithNonExistingTour_ShouldThrowNotFoundException()
    {
        // Arrange
        var tour = new Tour { Id = 999, TourName = "Non-existing Tour" };
        _mockTourRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tour?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _tourService.UpdateTourAsync(tour));
    }

    [Fact]
    public async Task UpdateTourAsync_ShouldSetModifiedDateToUtcNow()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        var beforeUpdate = DateTime.UtcNow;
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tour { Id = 1 });
        _mockTourRepository.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _tourService.UpdateTourAsync(tour);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(tour.ModifiedDate);
        Assert.True(tour.ModifiedDate >= beforeUpdate && tour.ModifiedDate <= afterUpdate);
    }

    [Fact]
    public async Task DeleteTourAsync_WithExistingTour_ShouldDeleteTour()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tour { Id = 1 });
        _mockTourRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _tourService.DeleteTourAsync(1);

        // Assert
        _mockTourRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTourAsync_WithNonExistingTour_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tour?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _tourService.DeleteTourAsync(999));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(999)]
    public async Task DeleteTourAsync_WithDifferentIds_ShouldCallRepository(int id)
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tour { Id = id });
        _mockTourRepository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _tourService.DeleteTourAsync(id);

        // Assert
        _mockTourRepository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchToursAsync_WithValidSearchTerm_ShouldReturnMatchingTours()
    {
        // Arrange
        var searchTerm = "Paris";
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Paris Adventure" },
            new Tour { Id = 2, TourName = "Paris Getaway" }
        };
        _mockTourRepository.Setup(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tours);

        // Act
        var result = await _tourService.SearchToursAsync(searchTerm);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchToursAsync_WithEmptySearchTerm_ShouldCallRepository()
    {
        // Arrange
        var searchTerm = string.Empty;
        _mockTourRepository.Setup(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tour>());

        // Act
        var result = await _tourService.SearchToursAsync(searchTerm);

        // Assert
        Assert.NotNull(result);
        _mockTourRepository.Verify(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Paris")]
    [InlineData("Rome")]
    [InlineData("Beach")]
    public async Task SearchToursAsync_WithDifferentSearchTerms_ShouldCallRepository(string searchTerm)
    {
        // Arrange
        _mockTourRepository.Setup(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tour>());

        // Act
        await _tourService.SearchToursAsync(searchTerm);

        // Assert
        _mockTourRepository.Verify(r => r.SearchAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveToursAsync_ShouldReturnActiveTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Active Tour 1", IsActive = true },
            new Tour { Id = 2, TourName = "Active Tour 2", IsActive = true }
        };
        _mockTourRepository.Setup(r => r.GetActiveToursAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tours);

        // Act
        var result = await _tourService.GetActiveToursAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, tour => Assert.True(tour.IsActive));
    }

    [Fact]
    public async Task GetActiveToursAsync_WithNoActiveTours_ShouldReturnEmptyList()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetActiveToursAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tour>());

        // Act
        var result = await _tourService.GetActiveToursAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllToursAsync_WhenRepositoryThrowsException_ShouldRethrowException()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _tourService.GetAllToursAsync());
    }

    [Fact]
    public async Task CreateTourAsync_WhenRepositoryThrowsException_ShouldRethrowException()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour" };
        _mockTourRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _tourService.CreateTourAsync(tour));
    }

    [Fact]
    public async Task UpdateTourAsync_WhenRepositoryThrowsException_ShouldRethrowException()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tour { Id = 1 });
        _mockTourRepository.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _tourService.UpdateTourAsync(tour));
    }

    [Fact]
    public async Task DeleteTourAsync_WhenRepositoryThrowsException_ShouldRethrowException()
    {
        // Arrange
        _mockTourRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tour { Id = 1 });
        _mockTourRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _tourService.DeleteTourAsync(1));
    }
}
