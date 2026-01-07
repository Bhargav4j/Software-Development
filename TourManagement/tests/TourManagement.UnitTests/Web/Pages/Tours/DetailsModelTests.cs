using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Web.Pages.Tours;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.UnitTests.Web.Pages.Tours;

public class DetailsModelTests
{
    private readonly Mock<ITourService> _mockTourService;
    private readonly Mock<ILogger<DetailsModel>> _mockLogger;
    private readonly DetailsModel _model;

    public DetailsModelTests()
    {
        _mockTourService = new Mock<ITourService>();
        _mockLogger = new Mock<ILogger<DetailsModel>>();
        _model = new DetailsModel(_mockTourService.Object, _mockLogger.Object);
    }

    [Fact]
    public void DetailsModel_Constructor_ShouldThrowWhenTourServiceIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DetailsModel(null!, _mockLogger.Object));
    }

    [Fact]
    public void DetailsModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DetailsModel(_mockTourService.Object, null!));
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPageResult_WhenTourExists()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        _mockTourService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tour);

        // Act
        var result = await _model.OnGetAsync(1);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.Tour);
        Assert.Equal("Test Tour", _model.Tour.TourName);
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPageResult_WhenTourNotFound()
    {
        // Arrange
        _mockTourService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Tour?)null);

        // Act
        var result = await _model.OnGetAsync(999);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_model.Tour);
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToError_WhenExceptionOccurs()
    {
        // Arrange
        _mockTourService.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _model.OnGetAsync(1);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        var redirectResult = result as RedirectToPageResult;
        Assert.Equal("/Error", redirectResult!.PageName);
    }

    [Fact]
    public async Task OnGetAsync_ShouldLogWarning_WhenTourNotFound()
    {
        // Arrange
        _mockTourService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Tour?)null);

        // Act
        await _model.OnGetAsync(999);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
