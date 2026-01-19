using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Repositories.Tests;

public class UserRepositoryTests
{
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly DbContextOptions<TourManagementDbContext> _options;
    private readonly TourManagementDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<UserRepository>>();
        _options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TourManagementDbContext(_options);
        _repository = new UserRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserRepository(null!, _mockLogger.Object));
        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserRepository(_context, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange, Act & Assert
        Assert.NotNull(_repository);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "user1@test.com", IsActive = true },
            new User { Email = "user2@test.com", IsActive = true }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.True(u.IsActive));
    }

    [Fact]
    public async Task GetAllAsync_ShouldNotReturnInactiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "active@test.com", IsActive = true },
            new User { Email = "inactive@test.com", IsActive = false }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("active@test.com", result.First().Email);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsersOrderedByEmail()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "charlie@test.com", IsActive = true },
            new User { Email = "alice@test.com", IsActive = true },
            new User { Email = "bob@test.com", IsActive = true }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var usersList = result.ToList();
        Assert.Equal("alice@test.com", usersList[0].Email);
        Assert.Equal("bob@test.com", usersList[1].Email);
        Assert.Equal("charlie@test.com", usersList[2].Email);
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
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("test@test.com", result.Email);
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
    public async Task GetByIdAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var user = new User { Email = "inactive@test.com", IsActive = false };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Email = "test@test.com", FirstName = "John", IsActive = true };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("John", result.FirstName);
    }

    [Fact]
    public async Task GetByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var user = new User { Email = "inactive@test.com", IsActive = false };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("inactive@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = new User
        {
            Email = "newuser@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        var savedUser = await _context.Users.FindAsync(result.Id);
        Assert.NotNull(savedUser);
        Assert.Equal("newuser@test.com", savedUser.Email);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnUserWithGeneratedId()
    {
        // Arrange
        var user = new User { Email = "test@test.com", IsActive = true };

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingUser()
    {
        // Arrange
        var user = new User
        {
            Email = "original@test.com",
            FirstName = "John",
            IsActive = true
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.Entry(user).State = EntityState.Detached;

        user.FirstName = "Updated";

        // Act
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyMultipleProperties()
    {
        // Arrange
        var user = new User
        {
            Email = "original@test.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1234567890",
            IsActive = true
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.Entry(user).State = EntityState.Detached;

        user.FirstName = "Jane";
        user.LastName = "Smith";
        user.PhoneNumber = "0987654321";

        // Act
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.Equal("Jane", updatedUser!.FirstName);
        Assert.Equal("Smith", updatedUser.LastName);
        Assert.Equal("0987654321", updatedUser.PhoneNumber);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteUser()
    {
        // Arrange
        var user = new User { Email = "todelete@test.com", IsActive = true };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        // Act
        await _repository.DeleteAsync(userId);

        // Assert
        var deletedUser = await _context.Users.FindAsync(userId);
        Assert.NotNull(deletedUser);
        Assert.False(deletedUser.IsActive);
        Assert.NotNull(deletedUser.ModifiedDate);
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
        var user = new User { Email = "todelete@test.com", IsActive = true };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;
        var beforeDelete = DateTime.UtcNow;

        // Act
        await _repository.DeleteAsync(userId);
        var afterDelete = DateTime.UtcNow;

        // Assert
        var deletedUser = await _context.Users.FindAsync(userId);
        Assert.NotNull(deletedUser!.ModifiedDate);
        Assert.True(deletedUser.ModifiedDate >= beforeDelete && deletedUser.ModifiedDate <= afterDelete);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingActiveUser_ShouldReturnTrue()
    {
        // Arrange
        var user = new User { Email = "test@test.com", IsActive = true };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(user.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        var user = new User { Email = "inactive@test.com", IsActive = false };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(user.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var user = new User { Email = "existing@test.com", IsActive = true };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("existing@test.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.EmailExistsAsync("nonexistent@test.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EmailExistsAsync_WithInactiveUserEmail_ShouldReturnFalse()
    {
        // Arrange
        var user = new User { Email = "inactive@test.com", IsActive = false };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("inactive@test.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingEmail_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "john@test.com", FirstName = "John", LastName = "Doe", IsActive = true },
            new User { Email = "jane@test.com", FirstName = "Jane", LastName = "Smith", IsActive = true },
            new User { Email = "bob@example.com", FirstName = "Bob", LastName = "Jones", IsActive = true }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("test.com");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_WithMatchingFirstName_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "john1@test.com", FirstName = "John", LastName = "Doe", IsActive = true },
            new User { Email = "john2@test.com", FirstName = "John", LastName = "Smith", IsActive = true },
            new User { Email = "jane@test.com", FirstName = "Jane", LastName = "Doe", IsActive = true }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("John");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_WithMatchingLastName_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "john@test.com", FirstName = "John", LastName = "Doe", IsActive = true },
            new User { Email = "jane@test.com", FirstName = "Jane", LastName = "Doe", IsActive = true },
            new User { Email = "bob@test.com", FirstName = "Bob", LastName = "Smith", IsActive = true }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Doe");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_WithEmptySearchTerm_ShouldReturnAllActiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "user1@test.com", IsActive = true },
            new User { Email = "user2@test.com", IsActive = true }
        };
        await _context.Users.AddRangeAsync(users);
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
        var user = new User
        {
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("JOHN");

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldNotReturnInactiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "john1@test.com", FirstName = "John", IsActive = true },
            new User { Email = "john2@test.com", FirstName = "John", IsActive = false }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("John");

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsActive);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnUsersOrderedByEmail()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "charlie@test.com", FirstName = "Test", IsActive = true },
            new User { Email = "alice@test.com", FirstName = "Test", IsActive = true },
            new User { Email = "bob@test.com", FirstName = "Test", IsActive = true }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Test");

        // Assert
        var usersList = result.ToList();
        Assert.Equal("alice@test.com", usersList[0].Email);
        Assert.Equal("bob@test.com", usersList[1].Email);
        Assert.Equal("charlie@test.com", usersList[2].Email);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
