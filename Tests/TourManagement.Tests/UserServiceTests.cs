using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using Xunit;

namespace TourManagement.Application.Services.Tests
{
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
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserService(_mockRepository.Object, null!));
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Email = "user1@test.com" },
                new User { Id = 2, Email = "user2@test.com" }
            };
            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<User>)result).Count);
            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("test@example.com", result.Email);
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            _mockRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
            _mockRepository.Verify(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WithNewUser_ShouldCreateUser()
        {
            // Arrange
            var user = new User { Email = "newuser@test.com", FullName = "New User" };
            _mockRepository.Setup(r => r.GetByEmailAsync("newuser@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.CreateUserAsync(user, "password123");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsActive);
            Assert.Equal("System", result.CreatedBy);
            Assert.NotEqual(default(DateTime), result.CreatedDate);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var existingUser = new User { Id = 1, Email = "existing@test.com" };
            var newUser = new User { Email = "existing@test.com" };
            _mockRepository.Setup(r => r.GetByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUserAsync(newUser, "password123"));
        }

        [Fact]
        public async Task UpdateUserAsync_WithExistingUser_ShouldUpdateUser()
        {
            // Arrange
            var existingUser = new User { Id = 1, Email = "old@test.com" };
            var updatedUser = new User { Email = "updated@test.com", FullName = "Updated User" };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateUserAsync(1, updatedUser);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithNonExistentUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var updatedUser = new User { Email = "updated@test.com" };
            _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateUserAsync(999, updatedUser));
        }

        [Fact]
        public async Task DeleteUserAsync_WithExistingUser_ShouldDeleteUser()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteUserAsync(1);

            // Assert
            _mockRepository.Verify(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithNonExistentUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteUserAsync(999));
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnUser()
        {
            // Arrange
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                IsActive = true
            };
            _mockRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.AuthenticateAsync("test@example.com", password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correct");
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                IsActive = true
            };
            _mockRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.AuthenticateAsync("test@example.com", "wrong");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInactiveUser_ShouldReturnNull()
        {
            // Arrange
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                IsActive = false
            };
            _mockRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _service.AuthenticateAsync("test@example.com", password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByEmailAsync("nonexistent@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _service.AuthenticateAsync("nonexistent@test.com", "password");

            // Assert
            Assert.Null(result);
        }
    }
}
