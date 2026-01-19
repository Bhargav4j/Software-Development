using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Repositories.Tests;

public class TourRepositoryTests
{
    private readonly Mock<ILogger<TourRepository>> _mockLogger;
    private readonly DbContextOptions<TourManagementDbContext> _options;
    private readonly TourManagementDbContext _context;
    private readonly TourRepository _repository;

    public TourRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<TourRepository>>();
        _options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TourManagementDbContext(_options);
        _repository = new TourRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TourRepository(null!, _mockLogger.Object));
        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TourRepository(_context, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange, Act & Assert
        Assert.NotNull(_repository);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Tour 1", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { Id = 2, TourName = "Tour 2", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-1) }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotReturnInactiveTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Active Tour", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { Id = 2, TourName = "Inactive Tour", IsActive = false, CreatedDate = DateTime.UtcNow }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Tour", result.First().TourName);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnToursOrderedByCreatedDateDescending()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Old Tour", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-10) },
            new Tour { Id = 2, TourName = "New Tour", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { Id = 3, TourName = "Middle Tour", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-5) }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var toursList = result.ToList();
        Assert.Equal("New Tour", toursList[0].TourName);
        Assert.Equal("Middle Tour", toursList[1].TourName);
        Assert.Equal("Old Tour", toursList[2].TourName);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTour()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Tour", result.TourName);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveTour_ShouldReturnNull()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Inactive Tour", IsActive = false };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTourToDatabase()
    {
        // Arrange
        var tour = new Tour { TourName = "New Tour", Place = "Paris", IsActive = true };

        // Act
        var result = await _repository.AddAsync(tour);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        var savedTour = await _context.Tours.FindAsync(result.Id);
        Assert.NotNull(savedTour);
        Assert.Equal("New Tour", savedTour.TourName);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnTourWithGeneratedId()
    {
        // Arrange
        var tour = new Tour { TourName = "Test Tour", IsActive = true };

        // Act
        var result = await _repository.AddAsync(tour);

        // Assert
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingTour()
    {
        // Arrange
        var tour = new Tour { TourName = "Original Name", Place = "Paris", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();
        _context.Entry(tour).State = EntityState.Detached;

        tour.TourName = "Updated Name";

        // Act
        await _repository.UpdateAsync(tour);

        // Assert
        var updatedTour = await _context.Tours.FindAsync(tour.Id);
        Assert.NotNull(updatedTour);
        Assert.Equal("Updated Name", updatedTour.TourName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyMultipleProperties()
    {
        // Arrange
        var tour = new Tour
        {
            TourName = "Original",
            Place = "Paris",
            Price = 1000m,
            Days = 5,
            IsActive = true
        };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();
        _context.Entry(tour).State = EntityState.Detached;

        tour.TourName = "Updated";
        tour.Place = "London";
        tour.Price = 1500m;
        tour.Days = 7;

        // Act
        await _repository.UpdateAsync(tour);

        // Assert
        var updatedTour = await _context.Tours.FindAsync(tour.Id);
        Assert.Equal("Updated", updatedTour!.TourName);
        Assert.Equal("London", updatedTour.Place);
        Assert.Equal(1500m, updatedTour.Price);
        Assert.Equal(7, updatedTour.Days);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteTour()
    {
        // Arrange
        var tour = new Tour { TourName = "Tour to Delete", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();
        var tourId = tour.Id;

        // Act
        await _repository.DeleteAsync(tourId);

        // Assert
        var deletedTour = await _context.Tours.FindAsync(tourId);
        Assert.NotNull(deletedTour);
        Assert.False(deletedTour.IsActive);
        Assert.NotNull(deletedTour.ModifiedDate);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrowException()
    {
        // Act & Assert
        await _repository.DeleteAsync(999);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetModifiedDate()
    {
        // Arrange
        var tour = new Tour { TourName = "Tour to Delete", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();
        var tourId = tour.Id;
        var beforeDelete = DateTime.UtcNow;

        // Act
        await _repository.DeleteAsync(tourId);
        var afterDelete = DateTime.UtcNow;

        // Assert
        var deletedTour = await _context.Tours.FindAsync(tourId);
        Assert.NotNull(deletedTour!.ModifiedDate);
        Assert.True(deletedTour.ModifiedDate >= beforeDelete && deletedTour.ModifiedDate <= afterDelete);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingActiveTour_ShouldReturnTrue()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentTour_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WithInactiveTour_ShouldReturnFalse()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Inactive Tour", IsActive = false };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingTourName_ShouldReturnMatchingTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { TourName = "Paris Adventure", Place = "France", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Paris Getaway", Place = "France", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "London Tour", Place = "England", IsActive = true, CreatedDate = DateTime.UtcNow }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Paris");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Contains("Paris", t.TourName));
    }

    [Fact]
    public async Task SearchAsync_WithMatchingPlace_ShouldReturnMatchingTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { TourName = "Tour 1", Place = "France", Locations = "Paris", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Tour 2", Place = "France", Locations = "Lyon", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Tour 3", Place = "England", Locations = "London", IsActive = true, CreatedDate = DateTime.UtcNow }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("France");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_WithMatchingLocations_ShouldReturnMatchingTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { TourName = "Tour 1", Place = "France", Locations = "Paris, Lyon", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Tour 2", Place = "Italy", Locations = "Rome, Milan", IsActive = true, CreatedDate = DateTime.UtcNow }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Milan");

        // Assert
        Assert.Single(result);
        Assert.Contains("Milan", result.First().Locations);
    }

    [Fact]
    public async Task SearchAsync_WithEmptySearchTerm_ShouldReturnAllActiveTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { TourName = "Tour 1", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Tour 2", IsActive = true, CreatedDate = DateTime.UtcNow }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(string.Empty);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var tour = new Tour { TourName = "Paris Adventure", Place = "France", IsActive = true, CreatedDate = DateTime.UtcNow };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("PARIS");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldNotReturnInactiveTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { TourName = "Paris Tour 1", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Paris Tour 2", IsActive = false, CreatedDate = DateTime.UtcNow }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Paris");

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsActive);
    }

    [Fact]
    public async Task GetActiveToursAsync_ShouldReturnOnlyActiveTours()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { TourName = "Active Tour 1", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Active Tour 2", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Inactive Tour", IsActive = false, CreatedDate = DateTime.UtcNow }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveToursAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task GetActiveToursAsync_ShouldReturnToursOrderedByCreatedDateDescending()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { TourName = "Old Tour", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-10) },
            new Tour { TourName = "New Tour", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Tour { TourName = "Middle Tour", IsActive = true, CreatedDate = DateTime.UtcNow.AddDays(-5) }
        };
        await _context.Tours.AddRangeAsync(tours);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveToursAsync();

        // Assert
        var toursList = result.ToList();
        Assert.Equal("New Tour", toursList[0].TourName);
        Assert.Equal("Middle Tour", toursList[1].TourName);
        Assert.Equal("Old Tour", toursList[2].TourName);
    }

    [Fact]
    public async Task GetActiveToursAsync_WithNoActiveTours_ShouldReturnEmptyList()
    {
        // Arrange
        var tour = new Tour { TourName = "Inactive Tour", IsActive = false, CreatedDate = DateTime.UtcNow };
        await _context.Tours.AddAsync(tour);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveToursAsync();

        // Assert
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
