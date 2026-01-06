using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Domain.Entities;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;
using Xunit;

namespace TourManagement.Infrastructure.Repositories.Tests
{
    public class TourRepositoryTests
    {
        private readonly TourManagementDbContext _context;
        private readonly Mock<ILogger<TourRepository>> _mockLogger;
        private readonly TourRepository _repository;

        public TourRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TourManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TourManagementDbContext(options);
            _mockLogger = new Mock<ILogger<TourRepository>>();
            _repository = new TourRepository(_context, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TourRepository(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TourRepository(_context, null!));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnActiveTours()
        {
            // Arrange
            _context.Tours.Add(new Tour { Id = 1, TourName = "Tour 1", IsActive = true });
            _context.Tours.Add(new Tour { Id = 2, TourName = "Tour 2", IsActive = true });
            _context.Tours.Add(new Tour { Id = 3, TourName = "Tour 3", IsActive = false });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnTour()
        {
            // Arrange
            _context.Tours.Add(new Tour { Id = 1, TourName = "Test Tour", IsActive = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Tour", result.TourName);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTour()
        {
            // Arrange
            var tour = new Tour { TourName = "New Tour", Place = "Paris", IsActive = true };

            // Act
            var result = await _repository.AddAsync(tour);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Tour", result.TourName);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTour()
        {
            // Arrange
            var tour = new Tour { Id = 1, TourName = "Old Tour", IsActive = true };
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            _context.Entry(tour).State = EntityState.Detached;

            tour.TourName = "Updated Tour";

            // Act
            await _repository.UpdateAsync(tour);

            // Assert
            var updatedTour = await _context.Tours.FindAsync(1);
            Assert.Equal("Updated Tour", updatedTour!.TourName);
        }

        [Fact]
        public async Task DeleteAsync_ShouldMarkTourAsInactive()
        {
            // Arrange
            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var deletedTour = await _context.Tours.FindAsync(1);
            Assert.NotNull(deletedTour);
            Assert.False(deletedTour.IsActive);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingTour_ShouldReturnTrue()
        {
            // Arrange
            _context.Tours.Add(new Tour { Id = 1, TourName = "Test Tour", IsActive = true });
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
        public async Task SearchAsync_ShouldReturnMatchingTours()
        {
            // Arrange
            _context.Tours.Add(new Tour { Id = 1, TourName = "Paris Tour", Place = "France", IsActive = true });
            _context.Tours.Add(new Tour { Id = 2, TourName = "London Tour", Place = "UK", IsActive = true });
            _context.Tours.Add(new Tour { Id = 3, TourName = "Rome Tour", Place = "Italy", Locations = "Paris included", IsActive = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("Paris");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchAsync_WithNoMatches_ShouldReturnEmptyList()
        {
            // Arrange
            _context.Tours.Add(new Tour { Id = 1, TourName = "London Tour", IsActive = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchAsync("Paris");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithInactiveTour_ShouldReturnNull()
        {
            // Arrange
            _context.Tours.Add(new Tour { Id = 1, TourName = "Inactive Tour", IsActive = false });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }
    }
}
