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
    public class AdminServiceTests
    {
        private readonly Mock<IAdminRepository> _mockRepository;
        private readonly Mock<ILogger<AdminService>> _mockLogger;
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            _mockRepository = new Mock<IAdminRepository>();
            _mockLogger = new Mock<ILogger<AdminService>>();
            _service = new AdminService(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AdminService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AdminService(_mockRepository.Object, null!));
        }

        [Fact]
        public async Task GetAllAdminsAsync_ShouldReturnAdmins()
        {
            // Arrange
            var admins = new List<Admin>
            {
                new Admin { Id = 1, Username = "admin1" },
                new Admin { Id = 2, Username = "admin2" }
            };
            _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(admins);

            // Act
            var result = await _service.GetAllAdminsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<Admin>)result).Count);
            _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAdminByIdAsync_ShouldReturnAdmin()
        {
            // Arrange
            var admin = new Admin { Id = 1, Username = "admin1" };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            // Act
            var result = await _service.GetAdminByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("admin1", result.Username);
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAdminByUsernameAsync_ShouldReturnAdmin()
        {
            // Arrange
            var admin = new Admin { Id = 1, Username = "testadmin" };
            _mockRepository.Setup(r => r.GetByUsernameAsync("testadmin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            // Act
            var result = await _service.GetAdminByUsernameAsync("testadmin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testadmin", result.Username);
            _mockRepository.Verify(r => r.GetByUsernameAsync("testadmin", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAdminAsync_WithNewAdmin_ShouldCreateAdmin()
        {
            // Arrange
            var admin = new Admin { Username = "newadmin", Email = "admin@test.com" };
            _mockRepository.Setup(r => r.GetByUsernameAsync("newadmin", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Admin?)null);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Admin>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            // Act
            var result = await _service.CreateAdminAsync(admin, "password123");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsActive);
            Assert.Equal("System", result.CreatedBy);
            Assert.NotEqual(default(DateTime), result.CreatedDate);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Admin>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAdminAsync_WithExistingUsername_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var existingAdmin = new Admin { Id = 1, Username = "existing" };
            var newAdmin = new Admin { Username = "existing" };
            _mockRepository.Setup(r => r.GetByUsernameAsync("existing", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAdmin);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAdminAsync(newAdmin, "password123"));
        }

        [Fact]
        public async Task UpdateAdminAsync_WithExistingAdmin_ShouldUpdateAdmin()
        {
            // Arrange
            var existingAdmin = new Admin { Id = 1, Username = "oldadmin" };
            var updatedAdmin = new Admin { Username = "updatedadmin", Email = "updated@test.com" };
            _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAdmin);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Admin>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAdminAsync(1, updatedAdmin);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Admin>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAdminAsync_WithNonExistentAdmin_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var updatedAdmin = new Admin { Username = "updated" };
            _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Admin?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAdminAsync(999, updatedAdmin));
        }

        [Fact]
        public async Task DeleteAdminAsync_WithExistingAdmin_ShouldDeleteAdmin()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAdminAsync(1);

            // Assert
            _mockRepository.Verify(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAdminAsync_WithNonExistentAdmin_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAdminAsync(999));
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnAdmin()
        {
            // Arrange
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var admin = new Admin
            {
                Id = 1,
                Username = "testadmin",
                PasswordHash = hashedPassword,
                IsActive = true
            };
            _mockRepository.Setup(r => r.GetByUsernameAsync("testadmin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            // Act
            var result = await _service.AuthenticateAsync("testadmin", password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testadmin", result.Username);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correct");
            var admin = new Admin
            {
                Id = 1,
                Username = "testadmin",
                PasswordHash = hashedPassword,
                IsActive = true
            };
            _mockRepository.Setup(r => r.GetByUsernameAsync("testadmin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            // Act
            var result = await _service.AuthenticateAsync("testadmin", "wrong");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInactiveAdmin_ShouldReturnNull()
        {
            // Arrange
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var admin = new Admin
            {
                Id = 1,
                Username = "testadmin",
                PasswordHash = hashedPassword,
                IsActive = false
            };
            _mockRepository.Setup(r => r.GetByUsernameAsync("testadmin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(admin);

            // Act
            var result = await _service.AuthenticateAsync("testadmin", password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNonExistentAdmin_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByUsernameAsync("nonexistent", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Admin?)null);

            // Act
            var result = await _service.AuthenticateAsync("nonexistent", "password");

            // Assert
            Assert.Null(result);
        }
    }
}
