using System;
using TourManagement.Domain.Entities;
using Xunit;

namespace TourManagement.Domain.Entities.Tests
{
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
            Assert.Equal(default(DateTime), booking.BookingDate);
            Assert.Equal(0, booking.NumberOfPeople);
            Assert.Equal(0m, booking.TotalAmount);
            Assert.Equal(string.Empty, booking.Status);
            Assert.Null(booking.Notes);
            Assert.Equal(default(DateTime), booking.CreatedDate);
            Assert.Null(booking.ModifiedDate);
            Assert.False(booking.IsActive);
            Assert.Equal(string.Empty, booking.CreatedBy);
            Assert.Null(booking.ModifiedBy);
        }

        [Fact]
        public void Booking_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var booking = new Booking();
            var bookingDate = DateTime.Now;
            var createdDate = DateTime.Now;
            var modifiedDate = DateTime.Now.AddDays(1);

            // Act
            booking.Id = 1;
            booking.UserId = 10;
            booking.TourId = 20;
            booking.BookingDate = bookingDate;
            booking.NumberOfPeople = 4;
            booking.TotalAmount = 5000.50m;
            booking.Status = "Confirmed";
            booking.Notes = "Window seats preferred";
            booking.CreatedDate = createdDate;
            booking.ModifiedDate = modifiedDate;
            booking.IsActive = true;
            booking.CreatedBy = "user1";
            booking.ModifiedBy = "admin";

            // Assert
            Assert.Equal(1, booking.Id);
            Assert.Equal(10, booking.UserId);
            Assert.Equal(20, booking.TourId);
            Assert.Equal(bookingDate, booking.BookingDate);
            Assert.Equal(4, booking.NumberOfPeople);
            Assert.Equal(5000.50m, booking.TotalAmount);
            Assert.Equal("Confirmed", booking.Status);
            Assert.Equal("Window seats preferred", booking.Notes);
            Assert.Equal(createdDate, booking.CreatedDate);
            Assert.Equal(modifiedDate, booking.ModifiedDate);
            Assert.True(booking.IsActive);
            Assert.Equal("user1", booking.CreatedBy);
            Assert.Equal("admin", booking.ModifiedBy);
        }

        [Fact]
        public void Booking_NumberOfPeople_ShouldAcceptPositiveValue()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.NumberOfPeople = 10;

            // Assert
            Assert.Equal(10, booking.NumberOfPeople);
        }

        [Fact]
        public void Booking_NumberOfPeople_ShouldAcceptZero()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.NumberOfPeople = 0;

            // Assert
            Assert.Equal(0, booking.NumberOfPeople);
        }

        [Fact]
        public void Booking_NumberOfPeople_ShouldAcceptNegativeValue()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.NumberOfPeople = -1;

            // Assert
            Assert.Equal(-1, booking.NumberOfPeople);
        }

        [Fact]
        public void Booking_TotalAmount_ShouldAcceptDecimalValue()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.TotalAmount = 1234.56m;

            // Assert
            Assert.Equal(1234.56m, booking.TotalAmount);
        }

        [Fact]
        public void Booking_TotalAmount_ShouldAcceptZero()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.TotalAmount = 0m;

            // Assert
            Assert.Equal(0m, booking.TotalAmount);
        }

        [Fact]
        public void Booking_TotalAmount_ShouldAcceptNegativeValue()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.TotalAmount = -100m;

            // Assert
            Assert.Equal(-100m, booking.TotalAmount);
        }

        [Fact]
        public void Booking_Status_ShouldAcceptDifferentStatuses()
        {
            // Arrange
            var booking = new Booking();

            // Act & Assert
            booking.Status = "Pending";
            Assert.Equal("Pending", booking.Status);

            booking.Status = "Confirmed";
            Assert.Equal("Confirmed", booking.Status);

            booking.Status = "Cancelled";
            Assert.Equal("Cancelled", booking.Status);

            booking.Status = "Completed";
            Assert.Equal("Completed", booking.Status);
        }

        [Fact]
        public void Booking_Notes_ShouldAcceptNull()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.Notes = null;

            // Assert
            Assert.Null(booking.Notes);
        }

        [Fact]
        public void Booking_Notes_ShouldAcceptLongText()
        {
            // Arrange
            var booking = new Booking();
            var longNotes = new string('A', 1000);

            // Act
            booking.Notes = longNotes;

            // Assert
            Assert.Equal(longNotes, booking.Notes);
        }

        [Fact]
        public void Booking_ModifiedBy_ShouldAcceptNull()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.ModifiedBy = null;

            // Assert
            Assert.Null(booking.ModifiedBy);
        }

        [Fact]
        public void Booking_ModifiedDate_ShouldAcceptNull()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.ModifiedDate = null;

            // Assert
            Assert.Null(booking.ModifiedDate);
        }

        [Fact]
        public void Booking_IsActive_ShouldToggle()
        {
            // Arrange
            var booking = new Booking { IsActive = false };

            // Act
            booking.IsActive = true;

            // Assert
            Assert.True(booking.IsActive);

            // Act
            booking.IsActive = false;

            // Assert
            Assert.False(booking.IsActive);
        }

        [Fact]
        public void Booking_NavigationProperties_ShouldBeSettable()
        {
            // Arrange
            var booking = new Booking();
            var user = new User { Id = 1, Email = "test@example.com" };
            var tour = new Tour { Id = 1, TourName = "Paris Tour" };

            // Act
            booking.User = user;
            booking.Tour = tour;

            // Assert
            Assert.NotNull(booking.User);
            Assert.Equal(user, booking.User);
            Assert.NotNull(booking.Tour);
            Assert.Equal(tour, booking.Tour);
        }

        [Fact]
        public void Booking_AllStringProperties_ShouldHandleEmptyStrings()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.Status = "";
            booking.CreatedBy = "";

            // Assert
            Assert.Equal("", booking.Status);
            Assert.Equal("", booking.CreatedBy);
        }

        [Fact]
        public void Booking_BookingDate_ShouldStoreDateTime()
        {
            // Arrange
            var booking = new Booking();
            var date = new DateTime(2023, 6, 15, 14, 30, 0);

            // Act
            booking.BookingDate = date;

            // Assert
            Assert.Equal(date, booking.BookingDate);
        }
    }
}
