using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Repositories.Tests
{
    public class TourRepositoryTests
    {
        private readonly Mock<ILogger<TourRepository>> _mockLogger;
        private readonly DbContextOptions<TourManagementDbContext> _dbContextOptions;

        public TourRepositoryTests()
        {
            _mockLogger = new Mock<ILogger<TourRepository>>();
            _dbContextOptions = new DbContextOptionsBuilder<TourManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private TourManagementDbContext CreateContext()
        {
            return new TourManagementDbContext(_dbContextOptions);
        }

        [Fact]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TourRepository(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            using var context = CreateContext();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TourRepository(context, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            using var context = CreateContext();

            // Act
            var repository = new TourRepository(context, _mockLogger.Object);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyActiveTours()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.AddRange(
                new Tour { Id = 1, TourName = "Active Tour 1", IsActive = true },
                new Tour { Id = 2, TourName = "Active Tour 2", IsActive = true },
                new Tour { Id = 3, TourName = "Inactive Tour", IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, tour => Assert.True(tour.IsActive));
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WithCancellationToken_UsesCancellationToken()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await repository.GetAllAsync(cancellationToken);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingActiveId_ReturnsTour()
        {
            // Arrange
            using var context = CreateContext();
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
        public async Task GetByIdAsync_WithInactiveId_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Inactive Tour", IsActive = false };
            context.Tours.Add(tour);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
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
        public async Task AddAsync_WithValidTour_AddsTourToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);
            var tour = new Tour { TourName = "New Tour", Place = "Paris", IsActive = true };

            // Act
            var result = await repository.AddAsync(tour);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Tour", result.TourName);

            var savedTour = await context.Tours.FindAsync(result.Id);
            Assert.NotNull(savedTour);
            Assert.Equal("New Tour", savedTour.TourName);
        }

        [Fact]
        public async Task AddAsync_WithMultipleTours_AddsAllToursToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);
            var tour1 = new Tour { TourName = "Tour 1", IsActive = true };
            var tour2 = new Tour { TourName = "Tour 2", IsActive = true };

            // Act
            await repository.AddAsync(tour1);
            await repository.AddAsync(tour2);

            // Assert
            var allTours = await repository.GetAllAsync();
            Assert.Equal(2, allTours.Count());
        }

        [Fact]
        public async Task UpdateAsync_WithExistingTour_UpdatesTour()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            var tour = new Tour { TourName = "Original Name", Place = "Paris", IsActive = true };
            context.Tours.Add(tour);
            await context.SaveChangesAsync();
            context.Entry(tour).State = EntityState.Detached;

            // Act
            tour.TourName = "Updated Name";
            await repository.UpdateAsync(tour);

            // Assert
            var updatedTour = await context.Tours.FindAsync(tour.Id);
            Assert.NotNull(updatedTour);
            Assert.Equal("Updated Name", updatedTour.TourName);
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_SetsIsActiveToFalse()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Tour to Delete", IsActive = true };
            context.Tours.Add(tour);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(1);

            // Assert
            var deletedTour = await context.Tours.FindAsync(1);
            Assert.NotNull(deletedTour);
            Assert.False(deletedTour.IsActive);
            Assert.NotNull(deletedTour.ModifiedDate);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_DoesNotThrowException()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            // Act & Assert
            await repository.DeleteAsync(999); // Should not throw
        }

        [Fact]
        public async Task DeleteAsync_SetsModifiedDateToUtcNow()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Tour", IsActive = true };
            context.Tours.Add(tour);
            await context.SaveChangesAsync();
            var beforeDelete = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await repository.DeleteAsync(1);
            var afterDelete = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var deletedTour = await context.Tours.FindAsync(1);
            Assert.NotNull(deletedTour);
            Assert.NotNull(deletedTour.ModifiedDate);
            Assert.True(deletedTour.ModifiedDate >= beforeDelete && deletedTour.ModifiedDate <= afterDelete);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingActiveId_ReturnsTrue()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            context.Tours.Add(tour);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithInactiveId_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Inactive Tour", IsActive = false };
            context.Tours.Add(tour);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.ExistsAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
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
        public async Task SearchAsync_WithMatchingTourName_ReturnsMatchingTours()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.AddRange(
                new Tour { Id = 1, TourName = "European Tour", Place = "Paris", IsActive = true },
                new Tour { Id = 2, TourName = "Asian Tour", Place = "Tokyo", IsActive = true },
                new Tour { Id = 3, TourName = "European Adventure", Place = "London", IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("European");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, tour => Assert.Contains("European", tour.TourName));
        }

        [Fact]
        public async Task SearchAsync_WithMatchingPlace_ReturnsMatchingTours()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.AddRange(
                new Tour { Id = 1, TourName = "Tour 1", Place = "Paris", IsActive = true },
                new Tour { Id = 2, TourName = "Tour 2", Place = "London", IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("Paris");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Paris", result.First().Place);
        }

        [Fact]
        public async Task SearchAsync_WithMatchingLocations_ReturnsMatchingTours()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.AddRange(
                new Tour { Id = 1, TourName = "Tour 1", Locations = "Paris, Rome, Berlin", IsActive = true },
                new Tour { Id = 2, TourName = "Tour 2", Locations = "Tokyo, Kyoto", IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("Rome");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("Rome", result.First().Locations);
        }

        [Fact]
        public async Task SearchAsync_WithEmptySearchTerm_ReturnsAllActiveTours()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.AddRange(
                new Tour { Id = 1, TourName = "Tour 1", IsActive = true },
                new Tour { Id = 2, TourName = "Tour 2", IsActive = true },
                new Tour { Id = 3, TourName = "Tour 3", IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchAsync_WithNullSearchTerm_ReturnsAllActiveTours()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.AddRange(
                new Tour { Id = 1, TourName = "Tour 1", IsActive = true },
                new Tour { Id = 2, TourName = "Tour 2", IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchAsync_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.Add(new Tour { Id = 1, TourName = "Tour 1", Place = "Paris", IsActive = true });
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("NonExistentTerm");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchAsync_OnlyReturnsActiveTours()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new TourRepository(context, _mockLogger.Object);

            context.Tours.AddRange(
                new Tour { Id = 1, TourName = "European Tour", IsActive = true },
                new Tour { Id = 2, TourName = "European Adventure", IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("European");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, tour => Assert.True(tour.IsActive));
        }
    }
}
