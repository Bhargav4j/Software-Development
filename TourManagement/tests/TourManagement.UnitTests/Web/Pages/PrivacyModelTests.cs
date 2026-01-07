using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TourManagement.Web.Pages;

namespace TourManagement.UnitTests.Web.Pages;

public class PrivacyModelTests
{
    private readonly Mock<ILogger<PrivacyModel>> _mockLogger;
    private readonly PrivacyModel _model;

    public PrivacyModelTests()
    {
        _mockLogger = new Mock<ILogger<PrivacyModel>>();
        _model = new PrivacyModel(_mockLogger.Object);
    }

    [Fact]
    public void PrivacyModel_Constructor_ShouldInitialize()
    {
        // Arrange & Act
        var model = new PrivacyModel(_mockLogger.Object);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void PrivacyModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PrivacyModel(null!));
    }

    [Fact]
    public void OnGet_ShouldExecuteWithoutError()
    {
        // Arrange & Act
        _model.OnGet();

        // Assert - method completes without exception
        Assert.NotNull(_model);
    }
}
