using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.Application.Services.Tests
{
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
        public void Constructor_WithNullUserRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UserService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UserService(_mockUserRepository.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var service = new UserService(_mockUserRepository.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { Id = 1, Email = "user1@test.com" },
                new User { Id = 2, Email = "user2@test.com" }
            };
            _mockUserRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUsers, result);
            _mockUserRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithCancellationToken_PassesCancellationToken()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedUsers = new List<User>();
            _mockUserRepository.Setup(r => r.GetAllAsync(cancellationToken))
                .ReturnsAsync(expectedUsers);

            // Act
            await _userService.GetAllUsersAsync(cancellationToken);

            // Assert
            _mockUserRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenRepositoryThrowsException_RethrowsException()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _userService.GetAllUsersAsync());
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User { Id = 1, Email = "test@example.com" };
            _mockUserRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUser, result);
            Assert.Equal(1, result.Id);
            _mockUserRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistentId_ReturnsNull()
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
        public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = new User { Id = 1, Email = email };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUser, result);
            Assert.Equal(email, result.Email);
            _mockUserRepository.Verify(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithNonExistentEmail_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUserRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUserAsync_WithNullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _userService.CreateUserAsync(null!));
        }

        [Fact]
        public async Task CreateUserAsync_WithExistingEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User { Email = "existing@example.com", PasswordHash = "password" };
            var existingUser = new User { Id = 1, Email = "existing@example.com" };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.CreateUserAsync(user));
        }

        [Fact]
        public async Task CreateUserAsync_WithValidUser_CreatesUserAndReturnsIt()
        {
            // Arrange
            var user = new User { Email = "new@example.com", PasswordHash = "password123" };
            var createdUser = new User { Id = 1, Email = "new@example.com" };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _userService.CreateUserAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.True(user.IsActive);
            Assert.NotEqual(default(DateTime), user.CreatedDate);
            Assert.NotEqual("password123", user.PasswordHash); // Password should be hashed
            _mockUserRepository.Verify(r => r.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_SetsCreatedDateToUtcNow()
        {
            // Arrange
            var user = new User { Email = "test@example.com", PasswordHash = "password" };
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
            _mockUserRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            await _userService.CreateUserAsync(user);
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.True(user.CreatedDate >= beforeCreation && user.CreatedDate <= afterCreation);
        }

        [Fact]
        public async Task CreateUserAsync_SetsIsActiveToTrue()
        {
            // Arrange
            var user = new User { Email = "test@example.com", PasswordHash = "password", IsActive = false };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            await _userService.CreateUserAsync(user);

            // Assert
            Assert.True(user.IsActive);
        }

        [Fact]
        public async Task CreateUserAsync_HashesPassword()
        {
            // Arrange
            var plainPassword = "myPlainPassword";
            var user = new User { Email = "test@example.com", PasswordHash = plainPassword };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            await _userService.CreateUserAsync(user);

            // Assert
            Assert.NotEqual(plainPassword, user.PasswordHash);
            Assert.NotEmpty(user.PasswordHash);
        }

        [Fact]
        public async Task UpdateUserAsync_WithNullUser_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _userService.UpdateUserAsync(null!));
        }

        [Fact]
        public async Task UpdateUserAsync_WithNonExistentUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User { Id = 999, Email = "nonexistent@example.com" };
            _mockUserRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.UpdateUserAsync(user));
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidUser_UpdatesUser()
        {
            // Arrange
            var user = new User { Id = 1, Email = "updated@example.com" };
            _mockUserRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.UpdateUserAsync(user);

            // Assert
            Assert.NotNull(user.ModifiedDate);
            _mockUserRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_SetsModifiedDateToUtcNow()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);
            _mockUserRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _userService.UpdateUserAsync(user);
            var afterUpdate = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.NotNull(user.ModifiedDate);
            Assert.True(user.ModifiedDate >= beforeUpdate && user.ModifiedDate <= afterUpdate);
        }

        [Fact]
        public async Task DeleteUserAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.DeleteUserAsync(999));
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidId_DeletesUser()
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
        public async Task AuthenticateAsync_WithNonExistentEmail_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password";
            _mockUserRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.AuthenticateAsync(email, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
            var user = new User { Id = 1, Email = email, PasswordHash = hashedPassword };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.AuthenticateAsync(email, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "correctpassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Id = 1, Email = email, PasswordHash = hashedPassword };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.AuthenticateAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user, result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenRepositoryThrowsException_RethrowsException()
        {
            // Arrange
            _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _userService.AuthenticateAsync("test@example.com", "password"));
        }
    }
}
