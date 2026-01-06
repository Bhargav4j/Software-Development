using Xunit;
using TourManagement.Domain.Entities;

namespace Tests.TourManagement.Domain.Entities;

/// <summary>
/// Tests for Tour entity
/// </summary>
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
        Assert.Null(tour.Locations);
        Assert.Null(tour.TourInfo);
        Assert.Null(tour.PicturePath);
        Assert.False(tour.IsActive);
        Assert.Equal("System", tour.CreatedBy);
        Assert.Null(tour.ModifiedBy);
        Assert.NotNull(tour.Bookings);
    }

    [Fact]
    public void Tour_SetProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var tour = new Tour();

        // Act
        tour.Id = 1;
        tour.TourName = "Paris Adventure";
        tour.Place = "Paris";
        tour.Days = 5;
        tour.Price = 999.99m;
        tour.Locations = "Eiffel Tower, Louvre, Notre Dame";
        tour.TourInfo = "A wonderful tour of Paris";
        tour.PicturePath = "/images/paris.jpg";
        tour.IsActive = true;
        tour.CreatedDate = DateTime.UtcNow;
        tour.CreatedBy = "Admin";

        // Assert
        Assert.Equal(1, tour.Id);
        Assert.Equal("Paris Adventure", tour.TourName);
        Assert.Equal("Paris", tour.Place);
        Assert.Equal(5, tour.Days);
        Assert.Equal(999.99m, tour.Price);
        Assert.Equal("Eiffel Tower, Louvre, Notre Dame", tour.Locations);
        Assert.Equal("A wonderful tour of Paris", tour.TourInfo);
        Assert.Equal("/images/paris.jpg", tour.PicturePath);
        Assert.True(tour.IsActive);
        Assert.Equal("Admin", tour.CreatedBy);
    }

    [Fact]
    public void Tour_Price_ShouldAcceptDecimalValues()
    {
        // Arrange & Act
        var tour = new Tour { Price = 1234.56m };

        // Assert
        Assert.Equal(1234.56m, tour.Price);
    }

    [Fact]
    public void Tour_Price_ShouldAcceptZero()
    {
        // Arrange & Act
        var tour = new Tour { Price = 0m };

        // Assert
        Assert.Equal(0m, tour.Price);
    }

    [Fact]
    public void Tour_Days_ShouldAcceptPositiveInteger()
    {
        // Arrange & Act
        var tour = new Tour { Days = 10 };

        // Assert
        Assert.Equal(10, tour.Days);
    }

    [Fact]
    public void Tour_OptionalFields_ShouldAcceptNullValues()
    {
        // Arrange & Act
        var tour = new Tour
        {
            Locations = null,
            TourInfo = null,
            PicturePath = null,
            ModifiedBy = null,
            ModifiedDate = null
        };

        // Assert
        Assert.Null(tour.Locations);
        Assert.Null(tour.TourInfo);
        Assert.Null(tour.PicturePath);
        Assert.Null(tour.ModifiedBy);
        Assert.Null(tour.ModifiedDate);
    }

    [Fact]
    public void Tour_Bookings_ShouldInitializeAsEmptyCollection()
    {
        // Arrange & Act
        var tour = new Tour();

        // Assert
        Assert.NotNull(tour.Bookings);
        Assert.Empty(tour.Bookings);
    }

    [Fact]
    public void Tour_Bookings_ShouldAllowAddingBookings()
    {
        // Arrange
        var tour = new Tour { Id = 1 };
        var booking = new Booking { Id = 1, TourId = tour.Id };

        // Act
        tour.Bookings.Add(booking);

        // Assert
        Assert.Single(tour.Bookings);
        Assert.Contains(booking, tour.Bookings);
    }

    [Fact]
    public void Tour_IsActive_ShouldToggleBetweenTrueAndFalse()
    {
        // Arrange
        var tour = new Tour { IsActive = true };

        // Act & Assert
        Assert.True(tour.IsActive);

        tour.IsActive = false;
        Assert.False(tour.IsActive);
    }

    [Fact]
    public void Tour_ModifiedDate_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var tour = new Tour();

        // Assert
        Assert.Null(tour.ModifiedDate);
    }

    [Fact]
    public void Tour_TourName_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var tour = new Tour { TourName = "" };

        // Assert
        Assert.Equal(string.Empty, tour.TourName);
    }

    [Fact]
    public void Tour_Place_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var tour = new Tour { Place = "" };

        // Assert
        Assert.Equal(string.Empty, tour.Place);
    }
}
