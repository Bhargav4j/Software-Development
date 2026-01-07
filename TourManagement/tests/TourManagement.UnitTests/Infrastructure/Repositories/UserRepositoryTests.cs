using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Infrastructure.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<ILogger<UserRepository>> _mockLogger;

    public UserRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<UserRepository>>();
    }

    private TourManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourManagementDbContext(options);
    }

    [Fact]
    public void UserRepository_Constructor_ShouldThrowWhenContextIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserRepository(null!, _mockLogger.Object));
    }

    [Fact]
    public void UserRepository_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange
        using var context = CreateContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserRepository(context, null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnActiveUsers()
    {
        // Arrange
        using var context = CreateContext();
        context.Users.AddRange(
            new User { Email = "user1@test.com", FirstName = "User1", LastName = "Test", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" },
            new User { Email = "user2@test.com", FirstName = "User2", LastName = "Test", Gender = "Female", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" },
            new User { Email = "inactive@test.com", FirstName = "Inactive", LastName = "Test", Gender = "Male", PasswordHash = "hash", IsActive = false, CreatedBy = "Test" }
        );
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.True(u.IsActive));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        using var context = CreateContext();
        var user = new User { Email = "test@test.com", FirstName = "John", LastName = "Doe", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotExists()
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
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        using var context = CreateContext();
        var user = new User { Email = "test@test.com", FirstName = "John", LastName = "Doe", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByEmailAsync("test@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailNotExists()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.GetByEmailAsync("nonexistent@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new UserRepository(context, _mockLogger.Object);
        var user = new User { Email = "new@test.com", FirstName = "New", LastName = "User", Gender = "Male", PasswordHash = "hash", CreatedBy = "Test" };

        // Act
        var result = await repository.AddAsync(user);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("new@test.com", result.Email);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        using var context = CreateContext();
        var user = new User { Email = "old@test.com", FirstName = "Old", LastName = "Name", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        user.FirstName = "New";
        await repository.UpdateAsync(user);

        // Assert
        var updatedUser = await context.Users.FindAsync(user.Id);
        Assert.Equal("New", updatedUser!.FirstName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkUserAsInactive()
    {
        // Arrange
        using var context = CreateContext();
        var user = new User { Email = "delete@test.com", FirstName = "Delete", LastName = "User", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        await repository.DeleteAsync(user.Id);

        // Assert
        var deletedUser = await context.Users.FindAsync(user.Id);
        Assert.NotNull(deletedUser);
        Assert.False(deletedUser.IsActive);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        using var context = CreateContext();
        var user = new User { Email = "test@test.com", FirstName = "Test", LastName = "User", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.ExistsAsync(user.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        using var context = CreateContext();
        var user = new User { Email = "existing@test.com", FirstName = "Test", LastName = "User", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.EmailExistsAsync("existing@test.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingUsers()
    {
        // Arrange
        using var context = CreateContext();
        context.Users.AddRange(
            new User { Email = "john@test.com", FirstName = "John", LastName = "Doe", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" },
            new User { Email = "johnny@test.com", FirstName = "Johnny", LastName = "Smith", Gender = "Male", PasswordHash = "hash", IsActive = true, CreatedBy = "Test" }
        );
        await context.SaveChangesAsync();
        var repository = new UserRepository(context, _mockLogger.Object);

        // Act
        var result = await repository.SearchAsync("john");

        // Assert
        Assert.Equal(2, result.Count());
    }
}
