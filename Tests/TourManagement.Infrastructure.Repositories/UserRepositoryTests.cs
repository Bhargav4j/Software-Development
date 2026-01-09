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
    public class UserRepositoryTests
    {
        private readonly Mock<ILogger<UserRepository>> _mockLogger;
        private readonly DbContextOptions<TourManagementDbContext> _dbContextOptions;

        public UserRepositoryTests()
        {
            _mockLogger = new Mock<ILogger<UserRepository>>();
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
                new UserRepository(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            using var context = CreateContext();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UserRepository(context, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            using var context = CreateContext();

            // Act
            var repository = new UserRepository(context, _mockLogger.Object);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyActiveUsers()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.AddRange(
                new User { Id = 1, Email = "user1@test.com", IsActive = true },
                new User { Id = 2, Email = "user2@test.com", IsActive = true },
                new User { Id = 3, Email = "user3@test.com", IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, user => Assert.True(user.IsActive));
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingActiveId_ReturnsUser()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithInactiveId_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "inactive@example.com", IsActive = false };
            context.Users.Add(user);
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
            var repository = new UserRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_WithExistingActiveEmail_ReturnsUser()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_WithInactiveEmail_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "inactive@example.com", IsActive = false };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByEmailAsync("inactive@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_WithValidUser_AddsUserToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);
            var user = new User { Email = "new@example.com", FirstName = "John", IsActive = true };

            // Act
            var result = await repository.AddAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("new@example.com", result.Email);

            var savedUser = await context.Users.FindAsync(result.Id);
            Assert.NotNull(savedUser);
            Assert.Equal("new@example.com", savedUser.Email);
        }

        [Fact]
        public async Task AddAsync_WithMultipleUsers_AddsAllUsersToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);
            var user1 = new User { Email = "user1@example.com", IsActive = true };
            var user2 = new User { Email = "user2@example.com", IsActive = true };

            // Act
            await repository.AddAsync(user1);
            await repository.AddAsync(user2);

            // Assert
            var allUsers = await repository.GetAllAsync();
            Assert.Equal(2, allUsers.Count());
        }

        [Fact]
        public async Task UpdateAsync_WithExistingUser_UpdatesUser()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Email = "original@example.com", FirstName = "Original", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            context.Entry(user).State = EntityState.Detached;

            // Act
            user.FirstName = "Updated";
            await repository.UpdateAsync(user);

            // Assert
            var updatedUser = await context.Users.FindAsync(user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal("Updated", updatedUser.FirstName);
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_SetsIsActiveToFalse()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "delete@example.com", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(1);

            // Assert
            var deletedUser = await context.Users.FindAsync(1);
            Assert.NotNull(deletedUser);
            Assert.False(deletedUser.IsActive);
            Assert.NotNull(deletedUser.ModifiedDate);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_DoesNotThrowException()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            // Act & Assert
            await repository.DeleteAsync(999); // Should not throw
        }

        [Fact]
        public async Task DeleteAsync_SetsModifiedDateToUtcNow()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var beforeDelete = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await repository.DeleteAsync(1);
            var afterDelete = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var deletedUser = await context.Users.FindAsync(1);
            Assert.NotNull(deletedUser);
            Assert.NotNull(deletedUser.ModifiedDate);
            Assert.True(deletedUser.ModifiedDate >= beforeDelete && deletedUser.ModifiedDate <= afterDelete);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingActiveId_ReturnsTrue()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Users.Add(user);
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
            var repository = new UserRepository(context, _mockLogger.Object);

            var user = new User { Id = 1, Email = "inactive@example.com", IsActive = false };
            context.Users.Add(user);
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
            var repository = new UserRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SearchAsync_WithMatchingEmail_ReturnsMatchingUsers()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.AddRange(
                new User { Id = 1, Email = "john@test.com", FirstName = "John", IsActive = true },
                new User { Id = 2, Email = "jane@test.com", FirstName = "Jane", IsActive = true },
                new User { Id = 3, Email = "john@example.com", FirstName = "Johnny", IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("john");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchAsync_WithMatchingFirstName_ReturnsMatchingUsers()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.AddRange(
                new User { Id = 1, Email = "user1@test.com", FirstName = "Alice", IsActive = true },
                new User { Id = 2, Email = "user2@test.com", FirstName = "Bob", IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("Alice");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Alice", result.First().FirstName);
        }

        [Fact]
        public async Task SearchAsync_WithMatchingLastName_ReturnsMatchingUsers()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.AddRange(
                new User { Id = 1, Email = "user1@test.com", FirstName = "John", LastName = "Smith", IsActive = true },
                new User { Id = 2, Email = "user2@test.com", FirstName = "Jane", LastName = "Doe", IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("Smith");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Smith", result.First().LastName);
        }

        [Fact]
        public async Task SearchAsync_WithEmptySearchTerm_ReturnsAllActiveUsers()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.AddRange(
                new User { Id = 1, Email = "user1@test.com", IsActive = true },
                new User { Id = 2, Email = "user2@test.com", IsActive = true },
                new User { Id = 3, Email = "user3@test.com", IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchAsync_WithNullSearchTerm_ReturnsAllActiveUsers()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.AddRange(
                new User { Id = 1, Email = "user1@test.com", IsActive = true },
                new User { Id = 2, Email = "user2@test.com", IsActive = true }
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
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.Add(new User { Id = 1, Email = "user1@test.com", FirstName = "John", IsActive = true });
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("NonExistentTerm");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchAsync_OnlyReturnsActiveUsers()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new UserRepository(context, _mockLogger.Object);

            context.Users.AddRange(
                new User { Id = 1, Email = "test@example.com", FirstName = "Test", IsActive = true },
                new User { Id = 2, Email = "test2@example.com", FirstName = "Test2", IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchAsync("test");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, user => Assert.True(user.IsActive));
        }
    }
}
