using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Domain.Entities;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;

namespace Tests.TourManagement.Infrastructure.Repositories;

/// <summary>
/// Tests for AdminRepository
/// </summary>
public class AdminRepositoryTests
{
    private readonly DbContextOptions<TourManagementDbContext> _options;
    private readonly Mock<ILogger<AdminRepository>> _mockLogger;

    public AdminRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockLogger = new Mock<ILogger<AdminRepository>>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveAdmins()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        context.Admins.AddRange(
            new Admin { Id = 1, Email = "admin1@test.com", IsActive = true },
            new Admin { Id = 2, Email = "admin2@test.com", IsActive = false },
            new Admin { Id = 3, Email = "admin3@test.com", IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.True(a.IsActive));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnAdmin()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Id = 1, Email = "admin@test.com", IsActive = true };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("admin@test.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveAdmin_ShouldReturnNull()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Id = 1, Email = "admin@test.com", IsActive = false };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnAdmin()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Email = "admin@test.com", IsActive = true };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByEmailAsync("admin@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin@test.com", result.Email);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAdminToDatabase()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);
        var admin = new Admin { Email = "newadmin@test.com", IsActive = true };

        // Act
        var result = await repository.AddAsync(admin);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAdmin()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Email = "admin@test.com", Name = "Original Name", IsActive = true };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        admin.Name = "Updated Name";
        await repository.UpdateAsync(admin);

        // Assert
        var updatedAdmin = await context.Admins.FindAsync(admin.Id);
        Assert.Equal("Updated Name", updatedAdmin!.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetAdminAsInactive()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Email = "admin@test.com", IsActive = true };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(admin.Id);

        // Assert
        var deletedAdmin = await context.Admins.FindAsync(admin.Id);
        Assert.False(deletedAdmin!.IsActive);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnAdmin()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Email = "admin@test.com", Password = "password123", IsActive = true };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.AuthenticateAsync("admin@test.com", "password123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin@test.com", result.Email);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Email = "admin@test.com", Password = "password123", IsActive = true };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.AuthenticateAsync("admin@test.com", "wrongpassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveAdmin_ShouldReturnNull()
    {
        // Arrange
        using var context = new TourManagementDbContext(_options);
        var repository = new AdminRepository(context, _mockLogger.Object);

        var admin = new Admin { Email = "admin@test.com", Password = "password123", IsActive = false };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.AuthenticateAsync("admin@test.com", "password123");

        // Assert
        Assert.Null(result);
    }
}
