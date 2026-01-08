using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using Xunit;

namespace TourManagement.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var users = new List<User>
        {
            new User { Id = 1, Email = "test1@example.com", FirstName = "Test1", LastName = "User1" },
            new User { Id = 2, Email = "test2@example.com", FirstName = "Test2", LastName = "User2" }
        };
        _userRepositoryMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(users);

        var result = await _userService.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var user = new User { Id = 1, Email = "test@example.com", FirstName = "Test", LastName = "User" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(user);

        var result = await _userService.GetByIdAsync(1);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((User?)null);

        var result = await _userService.GetByIdAsync(999);

        result.Should().BeNull();
    }
}
