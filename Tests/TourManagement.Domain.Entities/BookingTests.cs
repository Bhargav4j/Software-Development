using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Entities.Tests;

public class BookingTests
{
    [Fact]
    public void Booking_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var booking = new Booking();

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(0, booking.Id);
        Assert.Equal(0, booking.TourId);
        Assert.Equal(0, booking.UserId);
        Assert.Equal(0, booking.NumberOfPeople);
        Assert.Equal(0m, booking.TotalAmount);
        Assert.Equal("Pending", booking.Status);
        Assert.Equal("System", booking.CreatedBy);
        Assert.Null(booking.ModifiedBy);
        Assert.False(booking.IsActive);
    }

    [Fact]
    public void Booking_SetId_ShouldSetIdValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedId = 100;

        // Act
        booking.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, booking.Id);
    }

    [Fact]
    public void Booking_SetTourId_ShouldSetTourIdValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedTourId = 50;

        // Act
        booking.TourId = expectedTourId;

        // Assert
        Assert.Equal(expectedTourId, booking.TourId);
    }

    [Fact]
    public void Booking_SetUserId_ShouldSetUserIdValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedUserId = 25;

        // Act
        booking.UserId = expectedUserId;

        // Assert
        Assert.Equal(expectedUserId, booking.UserId);
    }

    [Fact]
    public void Booking_SetBookingDate_ShouldSetBookingDateValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedDate = new DateTime(2024, 12, 25);

        // Act
        booking.BookingDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, booking.BookingDate);
    }

    [Theory]
    [InlineData(2024, 1, 1)]
    [InlineData(2024, 6, 15)]
    [InlineData(2024, 12, 31)]
    public void Booking_SetBookingDate_ShouldHandleDifferentDates(int year, int month, int day)
    {
        // Arrange
        var booking = new Booking();
        var expectedDate = new DateTime(year, month, day);

        // Act
        booking.BookingDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, booking.BookingDate);
    }

    [Fact]
    public void Booking_SetNumberOfPeople_ShouldSetNumberOfPeopleValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedNumber = 4;

        // Act
        booking.NumberOfPeople = expectedNumber;

        // Assert
        Assert.Equal(expectedNumber, booking.NumberOfPeople);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void Booking_SetNumberOfPeople_ShouldHandleDifferentNumbers(int numberOfPeople)
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.NumberOfPeople = numberOfPeople;

        // Assert
        Assert.Equal(numberOfPeople, booking.NumberOfPeople);
    }

    [Fact]
    public void Booking_SetTotalAmount_ShouldSetTotalAmountValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedAmount = 1500.75m;

        // Act
        booking.TotalAmount = expectedAmount;

        // Assert
        Assert.Equal(expectedAmount, booking.TotalAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100.50)]
    [InlineData(1000.99)]
    [InlineData(9999.99)]
    public void Booking_SetTotalAmount_ShouldHandleDifferentAmounts(double amount)
    {
        // Arrange
        var booking = new Booking();
        var decimalAmount = (decimal)amount;

        // Act
        booking.TotalAmount = decimalAmount;

        // Assert
        Assert.Equal(decimalAmount, booking.TotalAmount);
    }

    [Fact]
    public void Booking_SetStatus_ShouldSetStatusValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedStatus = "Confirmed";

        // Act
        booking.Status = expectedStatus;

        // Assert
        Assert.Equal(expectedStatus, booking.Status);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Cancelled")]
    [InlineData("Completed")]
    public void Booking_SetStatus_ShouldHandleDifferentStatuses(string status)
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.Status = status;

        // Assert
        Assert.Equal(status, booking.Status);
    }

    [Fact]
    public void Booking_SetCreatedDate_ShouldSetCreatedDateValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedDate = new DateTime(2024, 1, 1);

        // Act
        booking.CreatedDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, booking.CreatedDate);
    }

    [Fact]
    public void Booking_SetModifiedDate_ShouldSetModifiedDateValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedDate = new DateTime(2024, 6, 15);

        // Act
        booking.ModifiedDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, booking.ModifiedDate);
    }

    [Fact]
    public void Booking_SetModifiedDate_ShouldAcceptNullValue()
    {
        // Arrange
        var booking = new Booking { ModifiedDate = DateTime.Now };

        // Act
        booking.ModifiedDate = null;

        // Assert
        Assert.Null(booking.ModifiedDate);
    }

    [Fact]
    public void Booking_SetIsActive_ShouldSetIsActiveValue()
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.IsActive = true;

        // Assert
        Assert.True(booking.IsActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Booking_SetIsActive_ShouldHandleBooleanValues(bool isActive)
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.IsActive = isActive;

        // Assert
        Assert.Equal(isActive, booking.IsActive);
    }

    [Fact]
    public void Booking_SetCreatedBy_ShouldSetCreatedByValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedUser = "AdminUser";

        // Act
        booking.CreatedBy = expectedUser;

        // Assert
        Assert.Equal(expectedUser, booking.CreatedBy);
    }

    [Fact]
    public void Booking_SetModifiedBy_ShouldSetModifiedByValue()
    {
        // Arrange
        var booking = new Booking();
        var expectedUser = "EditorUser";

        // Act
        booking.ModifiedBy = expectedUser;

        // Assert
        Assert.Equal(expectedUser, booking.ModifiedBy);
    }

    [Fact]
    public void Booking_SetModifiedBy_ShouldAcceptNullValue()
    {
        // Arrange
        var booking = new Booking { ModifiedBy = "User1" };

        // Act
        booking.ModifiedBy = null;

        // Assert
        Assert.Null(booking.ModifiedBy);
    }

    [Fact]
    public void Booking_SetTour_ShouldSetTourNavigationProperty()
    {
        // Arrange
        var booking = new Booking();
        var tour = new Tour { Id = 1, TourName = "Test Tour" };

        // Act
        booking.Tour = tour;

        // Assert
        Assert.NotNull(booking.Tour);
        Assert.Equal(tour.Id, booking.Tour.Id);
        Assert.Equal(tour.TourName, booking.Tour.TourName);
    }

    [Fact]
    public void Booking_SetUser_ShouldSetUserNavigationProperty()
    {
        // Arrange
        var booking = new Booking();
        var user = new User { Id = 1, Email = "test@test.com" };

        // Act
        booking.User = user;

        // Assert
        Assert.NotNull(booking.User);
        Assert.Equal(user.Id, booking.User.Id);
        Assert.Equal(user.Email, booking.User.Email);
    }

    [Fact]
    public void Booking_CompleteObject_ShouldSetAllProperties()
    {
        // Arrange
        var expectedId = 1;
        var expectedTourId = 10;
        var expectedUserId = 20;
        var expectedBookingDate = new DateTime(2024, 12, 25);
        var expectedNumberOfPeople = 4;
        var expectedTotalAmount = 2000.00m;
        var expectedStatus = "Confirmed";
        var expectedCreatedDate = new DateTime(2024, 1, 1);
        var expectedModifiedDate = new DateTime(2024, 6, 1);
        var expectedIsActive = true;
        var expectedCreatedBy = "Admin";
        var expectedModifiedBy = "Editor";

        // Act
        var booking = new Booking
        {
            Id = expectedId,
            TourId = expectedTourId,
            UserId = expectedUserId,
            BookingDate = expectedBookingDate,
            NumberOfPeople = expectedNumberOfPeople,
            TotalAmount = expectedTotalAmount,
            Status = expectedStatus,
            CreatedDate = expectedCreatedDate,
            ModifiedDate = expectedModifiedDate,
            IsActive = expectedIsActive,
            CreatedBy = expectedCreatedBy,
            ModifiedBy = expectedModifiedBy
        };

        // Assert
        Assert.Equal(expectedId, booking.Id);
        Assert.Equal(expectedTourId, booking.TourId);
        Assert.Equal(expectedUserId, booking.UserId);
        Assert.Equal(expectedBookingDate, booking.BookingDate);
        Assert.Equal(expectedNumberOfPeople, booking.NumberOfPeople);
        Assert.Equal(expectedTotalAmount, booking.TotalAmount);
        Assert.Equal(expectedStatus, booking.Status);
        Assert.Equal(expectedCreatedDate, booking.CreatedDate);
        Assert.Equal(expectedModifiedDate, booking.ModifiedDate);
        Assert.Equal(expectedIsActive, booking.IsActive);
        Assert.Equal(expectedCreatedBy, booking.CreatedBy);
        Assert.Equal(expectedModifiedBy, booking.ModifiedBy);
    }

    [Fact]
    public void Booking_SetEmptyStatus_ShouldAcceptEmptyString()
    {
        // Arrange
        var booking = new Booking { Status = "Pending" };

        // Act
        booking.Status = string.Empty;

        // Assert
        Assert.Equal(string.Empty, booking.Status);
    }

    [Fact]
    public void Booking_SetNegativeAmount_ShouldAcceptNegativeValue()
    {
        // Arrange
        var booking = new Booking();
        var negativeAmount = -100.50m;

        // Act
        booking.TotalAmount = negativeAmount;

        // Assert
        Assert.Equal(negativeAmount, booking.TotalAmount);
    }

    [Fact]
    public void Booking_SetZeroNumberOfPeople_ShouldAcceptZero()
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.NumberOfPeople = 0;

        // Assert
        Assert.Equal(0, booking.NumberOfPeople);
    }

    [Fact]
    public void Booking_SetNegativeNumberOfPeople_ShouldAcceptNegativeValue()
    {
        // Arrange
        var booking = new Booking();
        var negativeNumber = -5;

        // Act
        booking.NumberOfPeople = negativeNumber;

        // Assert
        Assert.Equal(negativeNumber, booking.NumberOfPeople);
    }

    [Fact]
    public void Booking_WithTourAndUser_ShouldMaintainRelationships()
    {
        // Arrange
        var tour = new Tour { Id = 1, TourName = "European Tour", Price = 1000m };
        var user = new User { Id = 1, FirstName = "John", LastName = "Doe" };
        var booking = new Booking
        {
            Id = 1,
            TourId = tour.Id,
            UserId = user.Id,
            NumberOfPeople = 2,
            TotalAmount = tour.Price * 2,
            Tour = tour,
            User = user
        };

        // Act & Assert
        Assert.Equal(tour.Id, booking.TourId);
        Assert.Equal(user.Id, booking.UserId);
        Assert.Equal(tour, booking.Tour);
        Assert.Equal(user, booking.User);
        Assert.Equal(2000m, booking.TotalAmount);
    }
}
