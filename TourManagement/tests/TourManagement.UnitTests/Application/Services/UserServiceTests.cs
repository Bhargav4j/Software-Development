using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.UnitTests.Application.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _service = new UserService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public void UserService_Constructor_ShouldThrowWhenRepositoryIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserService(null!, _mockLogger.Object));
    }

    [Fact]
    public void UserService_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserService(_mockRepository.Object, null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Email = "user1@test.com" },
            new User { Id = 2, Email = "user2@test.com" }
        };
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _service.GetByEmailAsync("test@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByEmailAsync("nonexistent@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenEmailDoesNotExist()
    {
        // Arrange
        var user = new User { Email = "new@test.com", FirstName = "John" };
        var createdUser = new User { Id = 1, Email = "new@test.com", FirstName = "John", IsActive = true };
        _mockRepository.Setup(r => r.EmailExistsAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdUser);

        // Act
        var result = await _service.CreateAsync(user, "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_ShouldHashPassword()
    {
        // Arrange
        var user = new User { Email = "test@test.com" };
        _mockRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken ct) => u);

        // Act
        var result = await _service.CreateAsync(user, "password123");

        // Assert
        Assert.NotNull(result.PasswordHash);
        Assert.NotEqual("password123", result.PasswordHash);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var user = new User { Email = "existing@test.com" };
        _mockRepository.Setup(r => r.EmailExistsAsync("existing@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(user, "password"));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var existingUser = new User { Id = 1, Email = "old@test.com" };
        var updatedUser = new User { Email = "new@test.com", FirstName = "Jane" };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(1, updatedUser);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenUserNotExists()
    {
        // Arrange
        var user = new User { Email = "test@test.com" };
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(999, user));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser_WhenUserExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenUserNotExists()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(999));
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnUser_WhenCredentialsAreValid()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = 1, Email = "test@test.com", PasswordHash = hashedPassword };
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _service.AuthenticateAsync("test@test.com", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _service.AuthenticateAsync("nonexistent@test.com", "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User { Email = "test@test.com", PasswordHash = hashedPassword };
        _mockRepository.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _service.AuthenticateAsync("test@test.com", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Email = "john@test.com", FirstName = "John" },
            new User { Id = 2, Email = "johnny@test.com", FirstName = "Johnny" }
        };
        _mockRepository.Setup(r => r.SearchAsync("john", It.IsAny<CancellationToken>())).ReturnsAsync(users);

        // Act
        var result = await _service.SearchAsync("john");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}
