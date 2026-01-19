using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Entities.Tests;

public class TourTests
{
    [Fact]
    public void Tour_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var tour = new Tour();

        // Assert
        Assert.NotNull(tour);
        Assert.Equal(0, tour.Id);
        Assert.Equal(string.Empty, tour.TourName);
        Assert.Equal(string.Empty, tour.Place);
        Assert.Equal(0, tour.Days);
        Assert.Equal(0m, tour.Price);
        Assert.Equal(string.Empty, tour.Locations);
        Assert.Equal(string.Empty, tour.TourInfo);
        Assert.Null(tour.PicturePath);
        Assert.Equal("System", tour.CreatedBy);
        Assert.Null(tour.ModifiedBy);
        Assert.False(tour.IsActive);
        Assert.NotNull(tour.Bookings);
        Assert.Empty(tour.Bookings);
    }

    [Fact]
    public void Tour_SetId_ShouldSetIdValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedId = 100;

        // Act
        tour.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, tour.Id);
    }

    [Fact]
    public void Tour_SetTourName_ShouldSetTourNameValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedName = "Paris Adventure";

        // Act
        tour.TourName = expectedName;

        // Assert
        Assert.Equal(expectedName, tour.TourName);
    }

    [Fact]
    public void Tour_SetPlace_ShouldSetPlaceValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedPlace = "France";

        // Act
        tour.Place = expectedPlace;

        // Assert
        Assert.Equal(expectedPlace, tour.Place);
    }

    [Fact]
    public void Tour_SetDays_ShouldSetDaysValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedDays = 7;

        // Act
        tour.Days = expectedDays;

        // Assert
        Assert.Equal(expectedDays, tour.Days);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(365)]
    public void Tour_SetDays_ShouldHandleDifferentDayValues(int days)
    {
        // Arrange
        var tour = new Tour();

        // Act
        tour.Days = days;

        // Assert
        Assert.Equal(days, tour.Days);
    }

    [Fact]
    public void Tour_SetPrice_ShouldSetPriceValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedPrice = 1500.50m;

        // Act
        tour.Price = expectedPrice;

        // Assert
        Assert.Equal(expectedPrice, tour.Price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100.50)]
    [InlineData(9999.99)]
    public void Tour_SetPrice_ShouldHandleDifferentPriceValues(double price)
    {
        // Arrange
        var tour = new Tour();
        var decimalPrice = (decimal)price;

        // Act
        tour.Price = decimalPrice;

        // Assert
        Assert.Equal(decimalPrice, tour.Price);
    }

    [Fact]
    public void Tour_SetLocations_ShouldSetLocationsValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedLocations = "Paris, Lyon, Nice";

        // Act
        tour.Locations = expectedLocations;

        // Assert
        Assert.Equal(expectedLocations, tour.Locations);
    }

    [Fact]
    public void Tour_SetTourInfo_ShouldSetTourInfoValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedInfo = "Amazing tour package with all amenities";

        // Act
        tour.TourInfo = expectedInfo;

        // Assert
        Assert.Equal(expectedInfo, tour.TourInfo);
    }

    [Fact]
    public void Tour_SetPicturePath_ShouldSetPicturePathValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedPath = "/images/paris.jpg";

        // Act
        tour.PicturePath = expectedPath;

        // Assert
        Assert.Equal(expectedPath, tour.PicturePath);
    }

    [Fact]
    public void Tour_SetPicturePath_ShouldAcceptNullValue()
    {
        // Arrange
        var tour = new Tour { PicturePath = "/images/test.jpg" };

        // Act
        tour.PicturePath = null;

        // Assert
        Assert.Null(tour.PicturePath);
    }

    [Fact]
    public void Tour_SetCreatedDate_ShouldSetCreatedDateValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedDate = new DateTime(2024, 1, 1);

        // Act
        tour.CreatedDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, tour.CreatedDate);
    }

    [Fact]
    public void Tour_SetModifiedDate_ShouldSetModifiedDateValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedDate = new DateTime(2024, 6, 15);

        // Act
        tour.ModifiedDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, tour.ModifiedDate);
    }

    [Fact]
    public void Tour_SetModifiedDate_ShouldAcceptNullValue()
    {
        // Arrange
        var tour = new Tour { ModifiedDate = DateTime.Now };

        // Act
        tour.ModifiedDate = null;

        // Assert
        Assert.Null(tour.ModifiedDate);
    }

    [Fact]
    public void Tour_SetIsActive_ShouldSetIsActiveValue()
    {
        // Arrange
        var tour = new Tour();

        // Act
        tour.IsActive = true;

        // Assert
        Assert.True(tour.IsActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tour_SetIsActive_ShouldHandleBooleanValues(bool isActive)
    {
        // Arrange
        var tour = new Tour();

        // Act
        tour.IsActive = isActive;

        // Assert
        Assert.Equal(isActive, tour.IsActive);
    }

    [Fact]
    public void Tour_SetCreatedBy_ShouldSetCreatedByValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedUser = "AdminUser";

        // Act
        tour.CreatedBy = expectedUser;

        // Assert
        Assert.Equal(expectedUser, tour.CreatedBy);
    }

    [Fact]
    public void Tour_SetModifiedBy_ShouldSetModifiedByValue()
    {
        // Arrange
        var tour = new Tour();
        var expectedUser = "EditorUser";

        // Act
        tour.ModifiedBy = expectedUser;

        // Assert
        Assert.Equal(expectedUser, tour.ModifiedBy);
    }

    [Fact]
    public void Tour_SetModifiedBy_ShouldAcceptNullValue()
    {
        // Arrange
        var tour = new Tour { ModifiedBy = "User1" };

        // Act
        tour.ModifiedBy = null;

        // Assert
        Assert.Null(tour.ModifiedBy);
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
        var tour = new Tour();
        var booking = new Booking { Id = 1, TourId = tour.Id };

        // Act
        tour.Bookings.Add(booking);

        // Assert
        Assert.Single(tour.Bookings);
        Assert.Contains(booking, tour.Bookings);
    }

    [Fact]
    public void Tour_CompleteObject_ShouldSetAllProperties()
    {
        // Arrange
        var expectedId = 1;
        var expectedName = "European Tour";
        var expectedPlace = "Europe";
        var expectedDays = 14;
        var expectedPrice = 5000.00m;
        var expectedLocations = "Paris, Rome, Berlin";
        var expectedInfo = "Complete European experience";
        var expectedPicturePath = "/images/europe.jpg";
        var expectedCreatedDate = new DateTime(2024, 1, 1);
        var expectedModifiedDate = new DateTime(2024, 6, 1);
        var expectedIsActive = true;
        var expectedCreatedBy = "Admin";
        var expectedModifiedBy = "Editor";

        // Act
        var tour = new Tour
        {
            Id = expectedId,
            TourName = expectedName,
            Place = expectedPlace,
            Days = expectedDays,
            Price = expectedPrice,
            Locations = expectedLocations,
            TourInfo = expectedInfo,
            PicturePath = expectedPicturePath,
            CreatedDate = expectedCreatedDate,
            ModifiedDate = expectedModifiedDate,
            IsActive = expectedIsActive,
            CreatedBy = expectedCreatedBy,
            ModifiedBy = expectedModifiedBy
        };

        // Assert
        Assert.Equal(expectedId, tour.Id);
        Assert.Equal(expectedName, tour.TourName);
        Assert.Equal(expectedPlace, tour.Place);
        Assert.Equal(expectedDays, tour.Days);
        Assert.Equal(expectedPrice, tour.Price);
        Assert.Equal(expectedLocations, tour.Locations);
        Assert.Equal(expectedInfo, tour.TourInfo);
        Assert.Equal(expectedPicturePath, tour.PicturePath);
        Assert.Equal(expectedCreatedDate, tour.CreatedDate);
        Assert.Equal(expectedModifiedDate, tour.ModifiedDate);
        Assert.Equal(expectedIsActive, tour.IsActive);
        Assert.Equal(expectedCreatedBy, tour.CreatedBy);
        Assert.Equal(expectedModifiedBy, tour.ModifiedBy);
    }

    [Fact]
    public void Tour_SetEmptyStrings_ShouldAcceptEmptyStrings()
    {
        // Arrange
        var tour = new Tour
        {
            TourName = "Test",
            Place = "TestPlace",
            Locations = "TestLocation",
            TourInfo = "TestInfo"
        };

        // Act
        tour.TourName = string.Empty;
        tour.Place = string.Empty;
        tour.Locations = string.Empty;
        tour.TourInfo = string.Empty;

        // Assert
        Assert.Equal(string.Empty, tour.TourName);
        Assert.Equal(string.Empty, tour.Place);
        Assert.Equal(string.Empty, tour.Locations);
        Assert.Equal(string.Empty, tour.TourInfo);
    }

    [Fact]
    public void Tour_SetNegativePrice_ShouldAcceptNegativeValue()
    {
        // Arrange
        var tour = new Tour();
        var negativePrice = -100.50m;

        // Act
        tour.Price = negativePrice;

        // Assert
        Assert.Equal(negativePrice, tour.Price);
    }

    [Fact]
    public void Tour_SetNegativeDays_ShouldAcceptNegativeValue()
    {
        // Arrange
        var tour = new Tour();
        var negativeDays = -5;

        // Act
        tour.Days = negativeDays;

        // Assert
        Assert.Equal(negativeDays, tour.Days);
    }
}
