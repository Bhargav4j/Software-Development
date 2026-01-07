using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using TourManagement.Web.Pages.Bookings;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.UnitTests.Web.Pages.Bookings;

public class IndexModelTests
{
    private readonly Mock<IBookingService> _mockBookingService;
    private readonly Mock<ILogger<IndexModel>> _mockLogger;
    private readonly Mock<ISession> _mockSession;
    private readonly IndexModel _model;

    public IndexModelTests()
    {
        _mockBookingService = new Mock<IBookingService>();
        _mockLogger = new Mock<ILogger<IndexModel>>();
        _mockSession = new Mock<ISession>();
        _model = new IndexModel(_mockBookingService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = _mockSession.Object;
        _model.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void IndexModel_Constructor_ShouldThrowWhenBookingServiceIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new IndexModel(null!, _mockLogger.Object));
    }

    [Fact]
    public void IndexModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new IndexModel(_mockBookingService.Object, null!));
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToLogin_WhenUserNotLoggedIn()
    {
        // Arrange
        byte[]? value = null;
        _mockSession.Setup(s => s.TryGetValue("UserEmail", out value)).Returns(false);

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        var redirectResult = result as RedirectToPageResult;
        Assert.Equal("/Users/Login", redirectResult!.PageName);
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPageResult_WhenUserIsLoggedIn()
    {
        // Arrange
        var email = "test@test.com";
        var emailBytes = System.Text.Encoding.UTF8.GetBytes(email);
        _mockSession.Setup(s => s.TryGetValue("UserEmail", out It.Ref<byte[]?>.IsAny!)).Returns(true);
        _mockSession.Setup(s => s.TryGetValue("UserEmail", out It.Ref<byte[]?>.IsAny!))
            .Callback(new TryGetValueCallback((string key, out byte[]? value) =>
            {
                value = emailBytes;
            }))
            .Returns(true);

        var bookings = new List<Booking>
        {
            new Booking { Id = 1, Email = email },
            new Booking { Id = 2, Email = email }
        };
        _mockBookingService.Setup(s => s.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(bookings);

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_ShouldSetMessage_WhenExceptionOccurs()
    {
        // Arrange
        var email = "test@test.com";
        var emailBytes = System.Text.Encoding.UTF8.GetBytes(email);
        _mockSession.Setup(s => s.TryGetValue("UserEmail", out It.Ref<byte[]?>.IsAny!))
            .Callback(new TryGetValueCallback((string key, out byte[]? value) =>
            {
                value = emailBytes;
            }))
            .Returns(true);
        _mockBookingService.Setup(s => s.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.Message);
    }

    private delegate void TryGetValueCallback(string key, out byte[]? value);
}
