using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Domain.Entities;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;

namespace Tests.TourManagement.Infrastructure.Repositories;

/// <summary>
/// Tests for TourRepository
/// </summary>
public class TourRepositoryTests
{
    private readonly DbContextOptions<TourManagementDbContext> _options;
    private readonly Mock<ILogger<TourRepository>> _mockLogger;

    public TourRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockLogger = new Mock<ILogger<TourRepository>>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveTours()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        context.Tours.AddRange(
            new Tour { Id = 1, TourName = "Paris Tour", IsActive = true },
            new Tour { Id = 2, TourName = "London Tour", IsActive = false },
            new Tour { Id = 3, TourName = "Rome Tour", IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTour()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Tour", result.TourName);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveTour_ShouldReturnNull()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = false };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTourToDatabase()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);
        var tour = new Tour { TourName = "New Tour", Place = "Paris", IsActive = true };

        // Act
        var result = await repository.AddAsync(tour);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTour()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        var tour = new Tour { TourName = "Original Tour", IsActive = true };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        // Act
        tour.TourName = "Updated Tour";
        await repository.UpdateAsync(tour);

        // Assert
        var updatedTour = await context.Tours.FindAsync(tour.Id);
        Assert.Equal("Updated Tour", updatedTour!.TourName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetTourAsInactive()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(tour.Id);

        // Assert
        var deletedTour = await context.Tours.FindAsync(tour.Id);
        Assert.False(deletedTour!.IsActive);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        var tour = new Tour { TourName = "Test Tour", IsActive = true };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.ExistsAsync(tour.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingTourName_ShouldReturnTours()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        context.Tours.AddRange(
            new Tour { TourName = "Paris Adventure", Place = "Paris", IsActive = true },
            new Tour { TourName = "Paris Explorer", Place = "Paris", IsActive = true },
            new Tour { TourName = "London Tour", Place = "London", IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.SearchAsync("Paris");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_WithMatchingPlace_ShouldReturnTours()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        context.Tours.AddRange(
            new Tour { TourName = "Tour A", Place = "Rome", IsActive = true },
            new Tour { TourName = "Tour B", Place = "Rome", IsActive = true },
            new Tour { TourName = "Tour C", Place = "Paris", IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.SearchAsync("Rome");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new TourRepository(context, _mockLogger.Object);

        context.Tours.Add(new Tour { TourName = "Test Tour", Place = "Paris", IsActive = true });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.SearchAsync("NonExistent");

        // Assert
        Assert.Empty(result);
    }
}
