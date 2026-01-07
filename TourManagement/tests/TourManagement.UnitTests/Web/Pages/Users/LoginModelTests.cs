using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using TourManagement.Web.Pages.Users;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.UnitTests.Web.Pages.Users;

public class LoginModelTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<LoginModel>> _mockLogger;
    private readonly Mock<ISession> _mockSession;
    private readonly LoginModel _model;

    public LoginModelTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<LoginModel>>();
        _mockSession = new Mock<ISession>();
        _model = new LoginModel(_mockUserService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));
        httpContext.Session = _mockSession.Object;
        _model.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void LoginModel_Constructor_ShouldThrowWhenUserServiceIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LoginModel(null!, _mockLogger.Object));
    }

    [Fact]
    public void LoginModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LoginModel(_mockUserService.Object, null!));
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
    public async Task OnPostAsync_ShouldRedirectToIndex_WhenAuthenticationSucceeds()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", FirstName = "John", LastName = "Doe" };
        _mockUserService.Setup(s => s.AuthenticateAsync("test@test.com", "password", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _model.Email = "test@test.com";
        _model.Password = "password";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        var redirectResult = result as RedirectToPageResult;
        Assert.Equal("/Index", redirectResult!.PageName);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetErrorMessage_WhenAuthenticationFails()
    {
        // Arrange
        _mockUserService.Setup(s => s.AuthenticateAsync("test@test.com", "wrongpassword", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _model.Email = "test@test.com";
        _model.Password = "wrongpassword";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.ErrorMessage);
        Assert.Contains("Invalid", _model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetErrorMessage_WhenExceptionOccurs()
    {
        // Arrange
        _mockUserService.Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));
        _model.Email = "test@test.com";
        _model.Password = "password";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.ErrorMessage);
    }
}
