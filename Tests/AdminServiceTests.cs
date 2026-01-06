using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;

namespace Tests.TourManagement.Application.Services;

/// <summary>
/// Tests for AdminService
/// </summary>
public class AdminServiceTests
{
    private readonly Mock<IAdminRepository> _mockAdminRepository;
    private readonly Mock<ILogger<AdminService>> _mockLogger;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _mockAdminRepository = new Mock<IAdminRepository>();
        _mockLogger = new Mock<ILogger<AdminService>>();
        _adminService = new AdminService(_mockAdminRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AuthenticateAdminAsync_WithValidCredentials_ShouldReturnAdmin()
    {
        // Arrange
        var admin = new Admin { Id = 1, Email = "admin@test.com", Password = "password123" };
        _mockAdminRepository.Setup(r => r.GetByEmailAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _adminService.AuthenticateAdminAsync("admin@test.com", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin@test.com", result.Email);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task AuthenticateAdminAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var admin = new Admin { Id = 1, Email = "admin@test.com", Password = "password123" };
        _mockAdminRepository.Setup(r => r.GetByEmailAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _adminService.AuthenticateAdminAsync("admin@test.com", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAdminAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        _mockAdminRepository.Setup(r => r.GetByEmailAsync("notfound@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Admin?)null);

        // Act
        var result = await _adminService.AuthenticateAdminAsync("notfound@test.com", "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAdminByIdAsync_WithValidId_ShouldReturnAdmin()
    {
        // Arrange
        var admin = new Admin { Id = 1, Email = "admin@test.com" };
        _mockAdminRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _adminService.GetAdminByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("admin@test.com", result.Email);
    }

    [Fact]
    public async Task GetAdminByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockAdminRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Admin?)null);

        // Act
        var result = await _adminService.GetAdminByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAdminAsync_WithEmptyEmail_ShouldReturnNull()
    {
        // Arrange
        _mockAdminRepository.Setup(r => r.GetByEmailAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Admin?)null);

        // Act
        var result = await _adminService.AuthenticateAdminAsync("", "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAdminAsync_WithEmptyPassword_ShouldReturnNull()
    {
        // Arrange
        var admin = new Admin { Id = 1, Email = "admin@test.com", Password = "password123" };
        _mockAdminRepository.Setup(r => r.GetByEmailAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _adminService.AuthenticateAdminAsync("admin@test.com", "");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAdminByIdAsync_WithZeroId_ShouldReturnNull()
    {
        // Arrange
        _mockAdminRepository.Setup(r => r.GetByIdAsync(0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Admin?)null);

        // Act
        var result = await _adminService.GetAdminByIdAsync(0);

        // Assert
        Assert.Null(result);
    }
}
