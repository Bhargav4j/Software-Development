using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Infrastructure.Repositories;

public class TourRepositoryTests
{
    private readonly Mock<ILogger<TourRepository>> _mockLogger;

    public TourRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<TourRepository>>();
    }

    private TourManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourManagementDbContext(options);
    }

    [Fact]
    public void TourRepository_Constructor_ShouldThrowWhenContextIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TourRepository(null!, _mockLogger.Object));
    }

    [Fact]
    public void TourRepository_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange
        using var context = CreateContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TourRepository(context, null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnActiveTours()
    {
        // Arrange
        using var context = CreateContext();
        context.Tours.AddRange(
            new Tour { TourName = "Tour 1", Place = "Place 1", IsActive = true, CreatedBy = "Test", Days = 1, Price = 100, Locations = "Loc1" },
            new Tour { TourName = "Tour 2", Place = "Place 2", IsActive = true, CreatedBy = "Test", Days = 2, Price = 200, Locations = "Loc2" },
            new Tour { TourName = "Inactive", Place = "Place 3", IsActive = false, CreatedBy = "Test", Days = 3, Price = 300, Locations = "Loc3" }
        );
        await context.SaveChangesAsync();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTour_WhenTourExists()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Test Tour", Place = "Test Place", IsActive = true, CreatedBy = "Test", Days = 5, Price = 500, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByIdAsync(tour.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Tour", result.TourName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTourNotExists()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTourIsInactive()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Inactive Tour", Place = "Place", IsActive = false, CreatedBy = "Test", Days = 1, Price = 100, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByIdAsync(tour.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTour()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new TourRepository(context, _mockLogger.Object);
        var tour = new Tour { TourName = "New Tour", Place = "New Place", CreatedBy = "Test", Days = 3, Price = 300, Locations = "Loc" };

        // Act
        var result = await repository.AddAsync(tour);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("New Tour", result.TourName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTour()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Old Name", Place = "Place", IsActive = true, CreatedBy = "Test", Days = 1, Price = 100, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        tour.TourName = "New Name";
        await repository.UpdateAsync(tour);

        // Assert
        var updatedTour = await context.Tours.FindAsync(tour.Id);
        Assert.Equal("New Name", updatedTour!.TourName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkTourAsInactive()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Tour to Delete", Place = "Place", IsActive = true, CreatedBy = "Test", Days = 1, Price = 100, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        await repository.DeleteAsync(tour.Id);

        // Assert
        var deletedTour = await context.Tours.FindAsync(tour.Id);
        Assert.NotNull(deletedTour);
        Assert.False(deletedTour.IsActive);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenTourExists()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Test Tour", Place = "Place", IsActive = true, CreatedBy = "Test", Days = 1, Price = 100, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.ExistsAsync(tour.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenTourNotExists()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.ExistsAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingTours()
    {
        // Arrange
        using var context = CreateContext();
        context.Tours.AddRange(
            new Tour { TourName = "Paris Tour", Place = "Paris", IsActive = true, CreatedBy = "Test", Days = 1, Price = 100, Locations = "Eiffel" },
            new Tour { TourName = "London Tour", Place = "Paris", IsActive = true, CreatedBy = "Test", Days = 2, Price = 200, Locations = "Big Ben" }
        );
        await context.SaveChangesAsync();
        var repository = new TourRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.SearchAsync("Paris");

        // Assert
        Assert.Equal(2, result.Count());
    }
}
