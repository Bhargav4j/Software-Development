using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using TourManagement.Web.Pages.Users;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.UnitTests.Web.Pages.Users;

public class RegisterModelTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<RegisterModel>> _mockLogger;
    private readonly RegisterModel _model;

    public RegisterModelTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<RegisterModel>>();
        _model = new RegisterModel(_mockUserService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _model.TempData = tempData;
        _model.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void RegisterModel_Constructor_ShouldThrowWhenUserServiceIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RegisterModel(null!, _mockLogger.Object));
    }

    [Fact]
    public void RegisterModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RegisterModel(_mockUserService.Object, null!));
    }

    [Fact]
    public void OnGet_ShouldExecuteWithoutError()
    {
        // Arrange & Act
        _model.OnGet();

        // Assert
        Assert.NotNull(_model);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        // Arrange
        _model.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToLogin_WhenRegistrationSucceeds()
    {
        // Arrange
        var user = new User { Id = 1, Email = "new@test.com" };
        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<User>(), "password123", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _model.Email = "new@test.com";
        _model.FirstName = "John";
        _model.LastName = "Doe";
        _model.Gender = "Male";
        _model.Password = "password123";
        _model.ConfirmPassword = "password123";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        var redirectResult = result as RedirectToPageResult;
        Assert.Equal("/Users/Login", redirectResult!.PageName);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetErrorMessage_WhenEmailAlreadyExists()
    {
        // Arrange
        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User with email already exists"));
        _model.Email = "existing@test.com";
        _model.FirstName = "John";
        _model.LastName = "Doe";
        _model.Gender = "Male";
        _model.Password = "password123";
        _model.ConfirmPassword = "password123";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.ErrorMessage);
        Assert.Contains("already exists", _model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetErrorMessage_WhenExceptionOccurs()
    {
        // Arrange
        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));
        _model.Email = "test@test.com";
        _model.FirstName = "John";
        _model.LastName = "Doe";
        _model.Gender = "Male";
        _model.Password = "password123";
        _model.ConfirmPassword = "password123";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        User? capturedUser = null;
        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<User, string, CancellationToken>((u, p, ct) => capturedUser = u)
            .ReturnsAsync(new User { Id = 1 });
        _model.Email = "test@test.com";
        _model.FirstName = "John";
        _model.LastName = "Doe";
        _model.Gender = "Male";
        _model.Password = "password123";
        _model.ConfirmPassword = "password123";

        // Act
        await _model.OnPostAsync();

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal("test@test.com", capturedUser!.Email);
        Assert.Equal("John", capturedUser.FirstName);
        Assert.Equal("Doe", capturedUser.LastName);
        Assert.Equal("Male", capturedUser.Gender);
    }
}
