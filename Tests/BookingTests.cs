using Xunit;
using TourManagement.Domain.Entities;

namespace Tests.TourManagement.Domain.Entities;

/// <summary>
/// Tests for Booking entity
/// </summary>
public class BookingTests
{
    [Fact]
    public void Booking_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var booking = new Booking();

        // Assert
        Assert.Equal(0, booking.Id);
        Assert.Equal(0, booking.UserId);
        Assert.Equal(0, booking.TourId);
        Assert.Equal(0, booking.NumberOfPeople);
        Assert.Equal(0, booking.TotalAmount);
        Assert.Equal("Pending", booking.Status);
        Assert.False(booking.IsActive);
        Assert.Equal("System", booking.CreatedBy);
        Assert.Null(booking.ModifiedBy);
    }

    [Fact]
    public void Booking_SetProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var booking = new Booking();
        var bookingDate = new DateTime(2024, 6, 15);
        var createdDate = DateTime.UtcNow;

        // Act
        booking.Id = 1;
        booking.UserId = 10;
        booking.TourId = 5;
        booking.BookingDate = bookingDate;
        booking.NumberOfPeople = 3;
        booking.TotalAmount = 2999.97m;
        booking.Status = "Confirmed";
        booking.CreatedDate = createdDate;
        booking.IsActive = true;
        booking.CreatedBy = "User";

        // Assert
        Assert.Equal(1, booking.Id);
        Assert.Equal(10, booking.UserId);
        Assert.Equal(5, booking.TourId);
        Assert.Equal(bookingDate, booking.BookingDate);
        Assert.Equal(3, booking.NumberOfPeople);
        Assert.Equal(2999.97m, booking.TotalAmount);
        Assert.Equal("Confirmed", booking.Status);
        Assert.True(booking.IsActive);
        Assert.Equal("User", booking.CreatedBy);
    }

    [Fact]
    public void Booking_Status_ShouldDefaultToPending()
    {
        // Arrange & Act
        var booking = new Booking();

        // Assert
        Assert.Equal("Pending", booking.Status);
    }

    [Fact]
    public void Booking_Status_ShouldAcceptDifferentValues()
    {
        // Arrange
        var booking = new Booking();

        // Act & Assert
        booking.Status = "Confirmed";
        Assert.Equal("Confirmed", booking.Status);

        booking.Status = "Cancelled";
        Assert.Equal("Cancelled", booking.Status);

        booking.Status = "Completed";
        Assert.Equal("Completed", booking.Status);
    }

    [Fact]
    public void Booking_TotalAmount_ShouldAcceptDecimalValues()
    {
        // Arrange & Act
        var booking = new Booking { TotalAmount = 1500.50m };

        // Assert
        Assert.Equal(1500.50m, booking.TotalAmount);
    }

    [Fact]
    public void Booking_TotalAmount_ShouldAcceptZero()
    {
        // Arrange & Act
        var booking = new Booking { TotalAmount = 0m };

        // Assert
        Assert.Equal(0m, booking.TotalAmount);
    }

    [Fact]
    public void Booking_NumberOfPeople_ShouldAcceptPositiveInteger()
    {
        // Arrange & Act
        var booking = new Booking { NumberOfPeople = 5 };

        // Assert
        Assert.Equal(5, booking.NumberOfPeople);
    }

    [Fact]
    public void Booking_User_ShouldAllowNavigationProperty()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        var booking = new Booking { UserId = 1, User = user };

        // Act & Assert
        Assert.NotNull(booking.User);
        Assert.Equal(1, booking.User.Id);
        Assert.Equal("test@test.com", booking.User.Email);
    }

    [Fact]
    public void Booking_Tour_ShouldAllowNavigationProperty()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "Paris Tour" };
        var booking = new Booking { TourId = 1, Tour = tour };

        // Act & Assert
        Assert.NotNull(booking.Tour);
        Assert.Equal(1, booking.Tour.Id);
        Assert.Equal("Paris Tour", booking.Tour.TourName);
    }

    [Fact]
    public void Booking_IsActive_ShouldToggleBetweenTrueAndFalse()
    {
        // Arrange
        var booking = new Booking { IsActive = true };

        // Act & Assert
        Assert.True(booking.IsActive);

        booking.IsActive = false;
        Assert.False(booking.IsActive);
    }

    [Fact]
    public void Booking_ModifiedDate_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var booking = new Booking();

        // Assert
        Assert.Null(booking.ModifiedDate);
    }

    [Fact]
    public void Booking_BookingDate_ShouldAcceptFutureDate()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(30);
        var booking = new Booking();

        // Act
        booking.BookingDate = futureDate;

        // Assert
        Assert.Equal(futureDate, booking.BookingDate);
    }

    [Fact]
    public void Booking_CreatedBy_ShouldDefaultToSystem()
    {
        // Arrange & Act
        var booking = new Booking();

        // Assert
        Assert.Equal("System", booking.CreatedBy);
    }
}
