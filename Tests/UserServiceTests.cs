using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Exceptions;

namespace Tests.TourManagement.Application.Services;

/// <summary>
/// Tests for UserService
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Email = "user1@test.com" },
            new User { Id = 2, Email = "user2@test.com" }
        };
        _mockUserRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidUser_ShouldReturnCreatedUser()
    {
        // Arrange
        var user = new User { Email = "newuser@test.com", Password = "password123" };
        _mockUserRepository.Setup(r => r.EmailExistsAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.CreateUserAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var user = new User { Email = "existing@test.com", Password = "password123" };
        _mockUserRepository.Setup(r => r.EmailExistsAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<TourManagementException>(() => _userService.CreateUserAsync(user));
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidUser_ShouldUpdateUser()
    {
        // Arrange
        var user = new User { Id = 1, Email = "updated@test.com" };
        _mockUserRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateUserAsync(user);

        // Assert
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = 999, Email = "notfound@test.com" };
        _mockUserRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _userService.UpdateUserAsync(user));
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _userService.DeleteUserAsync(1);

        // Assert
        _mockUserRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentId_ShouldThrowException()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _userService.DeleteUserAsync(999));
    }

    [Fact]
    public async Task AuthenticateUserAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = 1, Email = "test@test.com", Password = hashedPassword };
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.AuthenticateUserAsync("test@test.com", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task AuthenticateUserAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User { Id = 1, Email = "test@test.com", Password = hashedPassword };
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.AuthenticateUserAsync("test@test.com", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateUserAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetByEmailAsync("notfound@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.AuthenticateUserAsync("notfound@test.com", "password123");

        // Assert
        Assert.Null(result);
    }
}
