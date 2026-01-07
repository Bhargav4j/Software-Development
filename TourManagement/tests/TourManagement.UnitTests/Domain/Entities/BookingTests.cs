using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Domain.Entities;

public class BookingTests
{
    [Fact]
    public void Booking_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var booking = new Booking();

        // Assert
        Assert.Equal(0, booking.Id);
        Assert.Null(booking.TourId);
        Assert.Equal(string.Empty, booking.TourName);
        Assert.Equal(string.Empty, booking.Place);
        Assert.Equal(string.Empty, booking.Email);
        Assert.Equal(string.Empty, booking.FirstName);
        Assert.Equal(default(DateTime), booking.BookingDate);
        Assert.Equal(default(DateTime), booking.CreatedDate);
        Assert.Null(booking.ModifiedDate);
        Assert.False(booking.IsActive);
        Assert.Equal(string.Empty, booking.CreatedBy);
        Assert.Null(booking.ModifiedBy);
        Assert.Null(booking.Tour);
    }

    [Fact]
    public void Booking_Properties_ShouldBeSetAndGet()
    {
        // Arrange
        var booking = new Booking();
        var testDate = DateTime.UtcNow;

        // Act
        booking.Id = 1;
        booking.TourId = 5;
        booking.TourName = "Paris Tour";
        booking.Place = "Paris";
        booking.Email = "user@example.com";
        booking.FirstName = "John";
        booking.BookingDate = testDate;
        booking.CreatedDate = testDate;
        booking.ModifiedDate = testDate;
        booking.IsActive = true;
        booking.CreatedBy = "System";
        booking.ModifiedBy = "Admin";

        // Assert
        Assert.Equal(1, booking.Id);
        Assert.Equal(5, booking.TourId);
        Assert.Equal("Paris Tour", booking.TourName);
        Assert.Equal("Paris", booking.Place);
        Assert.Equal("user@example.com", booking.Email);
        Assert.Equal("John", booking.FirstName);
        Assert.Equal(testDate, booking.BookingDate);
        Assert.Equal(testDate, booking.CreatedDate);
        Assert.Equal(testDate, booking.ModifiedDate);
        Assert.True(booking.IsActive);
        Assert.Equal("System", booking.CreatedBy);
        Assert.Equal("Admin", booking.ModifiedBy);
    }

    [Fact]
    public void Booking_TourId_CanBeNull()
    {
        // Arrange
        var booking = new Booking
        {
            TourName = "Tour",
            Place = "Place",
            Email = "test@example.com",
            FirstName = "Test",
            CreatedBy = "System"
        };

        // Act & Assert
        Assert.Null(booking.TourId);
    }

    [Fact]
    public void Booking_Tour_CanBeAssigned()
    {
        // Arrange
        var booking = new Booking { Id = 1 };
        var tour = new Tour { Id = 5, TourName = "Test Tour" };

        // Act
        booking.Tour = tour;
        booking.TourId = tour.Id;

        // Assert
        Assert.NotNull(booking.Tour);
        Assert.Equal(tour, booking.Tour);
        Assert.Equal(5, booking.TourId);
    }

    [Fact]
    public void Booking_Email_ShouldAcceptValidFormat()
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.Email = "booking@domain.com";

        // Assert
        Assert.Equal("booking@domain.com", booking.Email);
    }

    [Fact]
    public void Booking_OptionalFields_CanBeNull()
    {
        // Arrange
        var booking = new Booking
        {
            TourName = "Tour",
            Place = "Place",
            Email = "test@example.com",
            FirstName = "Test",
            CreatedBy = "System"
        };

        // Assert
        Assert.Null(booking.TourId);
        Assert.Null(booking.ModifiedDate);
        Assert.Null(booking.ModifiedBy);
        Assert.Null(booking.Tour);
    }

    [Fact]
    public void Booking_BookingDate_ShouldStoreDateTime()
    {
        // Arrange
        var booking = new Booking();
        var bookingDate = new DateTime(2024, 1, 15, 10, 30, 0);

        // Act
        booking.BookingDate = bookingDate;

        // Assert
        Assert.Equal(bookingDate, booking.BookingDate);
    }
}
