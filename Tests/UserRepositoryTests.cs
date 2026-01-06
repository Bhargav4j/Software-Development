using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Domain.Entities;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;

namespace Tests.TourManagement.Infrastructure.Repositories;

/// <summary>
/// Tests for UserRepository
/// </summary>
public class UserRepositoryTests
{
    private readonly DbContextOptions<TourManagementDbContext> _options;
    private readonly Mock<ILogger<UserRepository>> _mockLogger;

    public UserRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockLogger = new Mock<ILogger<UserRepository>>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveUsers()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        context.Users.AddRange(
            new User { Id = 1, Email = "user1@test.com", IsActive = true },
            new User { Id = 2, Email = "user2@test.com", IsActive = false },
            new User { Id = 3, Email = "user3@test.com", IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.True(u.IsActive));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        var user = new User { Id = 1, Email = "test@test.com", IsActive = false };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        var user = new User { Email = "test@test.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByEmailAsync("test@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);
        var user = new User { Email = "newuser@test.com", IsActive = true };

        // Act
        var result = await repository.AddAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        var user = new User { Email = "test@test.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        user.Email = "updated@test.com";
        await repository.UpdateAsync(user);

        // Assert
        var updatedUser = await context.Users.FindAsync(user.Id);
        Assert.Equal("updated@test.com", updatedUser!.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetUserAsInactive()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        var user = new User { Email = "test@test.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(user.Id);

        // Assert
        var deletedUser = await context.Users.FindAsync(user.Id);
        Assert.False(deletedUser!.IsActive);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        var user = new User { Email = "test@test.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.ExistsAsync(user.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new UserRepository(context, _mockLogger.Object);

        var user = new User { Email = "test@test.com", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.EmailExistsAsync("test@test.com");

        // Assert
        Assert.True(result);
    }
}
