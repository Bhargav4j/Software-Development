using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TourManagement.Web.Pages.Tours;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.UnitTests.Web.Pages.Tours;

public class IndexModelTests
{
    private readonly Mock<ITourService> _mockTourService;
    private readonly Mock<ILogger<IndexModel>> _mockLogger;
    private readonly IndexModel _model;

    public IndexModelTests()
    {
        _mockTourService = new Mock<ITourService>();
        _mockLogger = new Mock<ILogger<IndexModel>>();
        _model = new IndexModel(_mockTourService.Object, _mockLogger.Object);
    }

    [Fact]
    public void IndexModel_Constructor_ShouldThrowWhenTourServiceIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new IndexModel(null!, _mockLogger.Object));
    }

    [Fact]
    public void IndexModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new IndexModel(_mockTourService.Object, null!));
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPageResult_WhenToursExist()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Tour 1" },
            new Tour { Id = 2, TourName = "Tour 2" }
        };
        _mockTourService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tours);

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(2, _model.Tours.Count());
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnEmptyList_WhenNoTours()
    {
        // Arrange
        _mockTourService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Tour>());

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Empty(_model.Tours);
    }

    [Fact]
    public async Task OnGetAsync_ShouldSetMessage_WhenExceptionOccurs()
    {
        // Arrange
        _mockTourService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.Message);
        Assert.Contains("error", _model.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OnGetAsync_ShouldLogInformation()
    {
        // Arrange
        var tours = new List<Tour> { new Tour { Id = 1 } };
        _mockTourService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tours);

        // Act
        await _model.OnGetAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
