using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Web.Pages;

namespace TourManagement.UnitTests.Web.Pages;

public class IndexModelTests
{
    private readonly Mock<ILogger<IndexModel>> _mockLogger;
    private readonly IndexModel _model;

    public IndexModelTests()
    {
        _mockLogger = new Mock<ILogger<IndexModel>>();
        _model = new IndexModel(_mockLogger.Object);
    }

    [Fact]
    public void IndexModel_Constructor_ShouldInitialize()
    {
        // Arrange & Act
        var model = new IndexModel(_mockLogger.Object);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void OnGet_ShouldExecuteWithoutError()
    {
        // Arrange & Act
        _model.OnGet();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Home page accessed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IndexModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new IndexModel(null!));
    }
}
