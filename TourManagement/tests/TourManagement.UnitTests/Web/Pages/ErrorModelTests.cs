using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Web.Pages;

namespace TourManagement.UnitTests.Web.Pages;

public class ErrorModelTests
{
    private readonly Mock<ILogger<ErrorModel>> _mockLogger;
    private readonly ErrorModel _model;

    public ErrorModelTests()
    {
        _mockLogger = new Mock<ILogger<ErrorModel>>();
        _model = new ErrorModel(_mockLogger.Object);
    }

    [Fact]
    public void ErrorModel_Constructor_ShouldInitialize()
    {
        // Arrange & Act
        var model = new ErrorModel(_mockLogger.Object);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void ErrorModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ErrorModel(null!));
    }

    [Fact]
    public void OnGet_ShouldSetRequestId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        _model.PageContext = new PageContext { HttpContext = httpContext };

        // Act
        _model.OnGet();

        // Assert
        Assert.NotNull(_model.RequestId);
        Assert.Equal("test-trace-id", _model.RequestId);
    }

    [Fact]
    public void ShowRequestId_ShouldReturnTrue_WhenRequestIdIsNotEmpty()
    {
        // Arrange
        _model.RequestId = "test-id";

        // Act
        var result = _model.ShowRequestId;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNull()
    {
        // Arrange
        _model.RequestId = null;

        // Act
        var result = _model.ShowRequestId;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsEmpty()
    {
        // Arrange
        _model.RequestId = string.Empty;

        // Act
        var result = _model.ShowRequestId;

        // Assert
        Assert.False(result);
    }
}
