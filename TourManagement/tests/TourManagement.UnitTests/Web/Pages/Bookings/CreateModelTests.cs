using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using TourManagement.Web.Pages.Bookings;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.UnitTests.Web.Pages.Bookings;

public class CreateModelTests
{
    private readonly Mock<IBookingService> _mockBookingService;
    private readonly Mock<ITourService> _mockTourService;
    private readonly Mock<ILogger<CreateModel>> _mockLogger;
    private readonly Mock<ISession> _mockSession;
    private readonly CreateModel _model;

    public CreateModelTests()
    {
        _mockBookingService = new Mock<IBookingService>();
        _mockTourService = new Mock<ITourService>();
        _mockLogger = new Mock<ILogger<CreateModel>>();
        _mockSession = new Mock<ISession>();
        _model = new CreateModel(_mockBookingService.Object, _mockTourService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = _mockSession.Object;
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _model.TempData = tempData;
        _model.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void CreateModel_Constructor_ShouldThrowWhenBookingServiceIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateModel(null!, _mockTourService.Object, _mockLogger.Object));
    }

    [Fact]
    public void CreateModel_Constructor_ShouldThrowWhenTourServiceIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateModel(_mockBookingService.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public void CreateModel_Constructor_ShouldThrowWhenLoggerIsNull()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateModel(_mockBookingService.Object, _mockTourService.Object, null!));
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPageResult_WhenTourExists()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        _mockTourService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tour);
        _model.TourId = 1;

        // Act
        var result = await _model.OnGetAsync();

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
        _model.TourId = 999;

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_model.Tour);
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirectToError_WhenExceptionOccurs()
    {
        // Arrange
        _mockTourService.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));
        _model.TourId = 1;

        // Act
        var result = await _model.OnGetAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        var redirectResult = result as RedirectToPageResult;
        Assert.Equal("/Error", redirectResult!.PageName);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenTourNotFound()
    {
        // Arrange
        _mockTourService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Tour?)null);
        _model.TourId = 999;

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour" };
        _mockTourService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tour);
        _model.TourId = 1;
        _model.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToBookingsIndex_WhenBookingSucceeds()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour", Place = "Test Place" };
        var booking = new Booking { Id = 1, Email = "test@test.com" };
        _mockTourService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tour);
        _mockBookingService.Setup(s => s.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _model.TourId = 1;
        _model.Email = "test@test.com";
        _model.FirstName = "John";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        var redirectResult = result as RedirectToPageResult;
        Assert.Equal("/Bookings/Index", redirectResult!.PageName);
    }

    [Fact]
    public async Task OnPostAsync_ShouldSetErrorMessage_WhenExceptionOccurs()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Test Tour", Place = "Test Place" };
        _mockTourService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tour);
        _mockBookingService.Setup(s => s.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));
        _model.TourId = 1;
        _model.Email = "test@test.com";
        _model.FirstName = "John";

        // Act
        var result = await _model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_ShouldCreateBookingWithCorrectProperties()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Paris Tour", Place = "Paris" };
        Booking? capturedBooking = null;
        _mockTourService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tour);
        _mockBookingService.Setup(s => s.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((b, ct) => capturedBooking = b)
            .ReturnsAsync(new Booking { Id = 1 });
        _model.TourId = 1;
        _model.Email = "test@test.com";
        _model.FirstName = "John";

        // Act
        await _model.OnPostAsync();

        // Assert
        Assert.NotNull(capturedBooking);
        Assert.Equal(1, capturedBooking!.TourId);
        Assert.Equal("Paris Tour", capturedBooking.TourName);
        Assert.Equal("Paris", capturedBooking.Place);
        Assert.Equal("test@test.com", capturedBooking.Email);
        Assert.Equal("John", capturedBooking.FirstName);
    }
}
