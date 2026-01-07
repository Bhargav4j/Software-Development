using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Domain.Entities;

public class TourTests
{
    [Fact]
    public void Tour_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var tour = new Tour();

        // Assert
        Assert.Equal(0, tour.Id);
        Assert.Equal(string.Empty, tour.TourName);
        Assert.Equal(string.Empty, tour.Place);
        Assert.Equal(0, tour.Days);
        Assert.Equal(0, tour.Price);
        Assert.Equal(string.Empty, tour.Locations);
        Assert.Null(tour.TourInfo);
        Assert.Null(tour.PicturePath);
        Assert.Equal(default(DateTime), tour.CreatedDate);
        Assert.Null(tour.ModifiedDate);
        Assert.False(tour.IsActive);
        Assert.Equal(string.Empty, tour.CreatedBy);
        Assert.Null(tour.ModifiedBy);
        Assert.NotNull(tour.Bookings);
        Assert.Empty(tour.Bookings);
    }

    [Fact]
    public void Tour_Properties_ShouldBeSetAndGet()
    {
        // Arrange
        var tour = new Tour();
        var testDate = DateTime.UtcNow;

        // Act
        tour.Id = 1;
        tour.TourName = "Paris Tour";
        tour.Place = "Paris";
        tour.Days = 7;
        tour.Price = 1500.50m;
        tour.Locations = "Eiffel Tower, Louvre";
        tour.TourInfo = "Amazing tour";
        tour.PicturePath = "/images/paris.jpg";
        tour.CreatedDate = testDate;
        tour.ModifiedDate = testDate;
        tour.IsActive = true;
        tour.CreatedBy = "Admin";
        tour.ModifiedBy = "User";

        // Assert
        Assert.Equal(1, tour.Id);
        Assert.Equal("Paris Tour", tour.TourName);
        Assert.Equal("Paris", tour.Place);
        Assert.Equal(7, tour.Days);
        Assert.Equal(1500.50m, tour.Price);
        Assert.Equal("Eiffel Tower, Louvre", tour.Locations);
        Assert.Equal("Amazing tour", tour.TourInfo);
        Assert.Equal("/images/paris.jpg", tour.PicturePath);
        Assert.Equal(testDate, tour.CreatedDate);
        Assert.Equal(testDate, tour.ModifiedDate);
        Assert.True(tour.IsActive);
        Assert.Equal("Admin", tour.CreatedBy);
        Assert.Equal("User", tour.ModifiedBy);
    }

    [Fact]
    public void Tour_Bookings_ShouldAcceptNewItems()
    {
        // Arrange
        var tour = new Tour { Id = 1 };
        var booking = new Booking { Id = 1, TourId = 1 };

        // Act
        tour.Bookings.Add(booking);

        // Assert
        Assert.Single(tour.Bookings);
        Assert.Contains(booking, tour.Bookings);
    }

    [Fact]
    public void Tour_Price_ShouldAcceptDecimalValues()
    {
        // Arrange
        var tour = new Tour();

        // Act
        tour.Price = 999.99m;

        // Assert
        Assert.Equal(999.99m, tour.Price);
    }

    [Fact]
    public void Tour_Days_ShouldAcceptPositiveIntegers()
    {
        // Arrange
        var tour = new Tour();

        // Act
        tour.Days = 5;

        // Assert
        Assert.Equal(5, tour.Days);
    }

    [Fact]
    public void Tour_OptionalFields_CanBeNull()
    {
        // Arrange
        var tour = new Tour
        {
            TourName = "Test Tour",
            Place = "Test Place",
            Locations = "Test Locations",
            CreatedBy = "System"
        };

        // Assert
        Assert.Null(tour.TourInfo);
        Assert.Null(tour.PicturePath);
        Assert.Null(tour.ModifiedDate);
        Assert.Null(tour.ModifiedBy);
    }
}
