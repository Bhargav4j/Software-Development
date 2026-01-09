using System;
using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Entities.Tests
{
    public class BookingTests
    {
        [Fact]
        public void Booking_Constructor_InitializesWithDefaultValues()
        {
            // Arrange & Act
            var booking = new Booking();

            // Assert
            Assert.Equal(0, booking.Id);
            Assert.Equal(0, booking.TourId);
            Assert.Equal(0, booking.UserId);
            Assert.Equal(default(DateTime), booking.BookingDate);
            Assert.Equal(0, booking.NumberOfPersons);
            Assert.Equal(0, booking.TotalAmount);
            Assert.Equal("Pending", booking.Status);
            Assert.Equal(default(DateTime), booking.CreatedDate);
            Assert.Null(booking.ModifiedDate);
            Assert.False(booking.IsActive);
            Assert.Equal("System", booking.CreatedBy);
            Assert.Null(booking.ModifiedBy);
        }

        [Fact]
        public void Booking_Id_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedId = 456;

            // Act
            booking.Id = expectedId;

            // Assert
            Assert.Equal(expectedId, booking.Id);
        }

        [Fact]
        public void Booking_TourId_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedTourId = 10;

            // Act
            booking.TourId = expectedTourId;

            // Assert
            Assert.Equal(expectedTourId, booking.TourId);
        }

        [Fact]
        public void Booking_UserId_CanBeSetAndRetrieved()
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
        public void Booking_BookingDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedDate = new DateTime(2024, 6, 15);

            // Act
            booking.BookingDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, booking.BookingDate);
        }

        [Fact]
        public void Booking_NumberOfPersons_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedPersons = 4;

            // Act
            booking.NumberOfPersons = expectedPersons;

            // Assert
            Assert.Equal(expectedPersons, booking.NumberOfPersons);
        }

        [Fact]
        public void Booking_TotalAmount_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedAmount = 3500.75m;

            // Act
            booking.TotalAmount = expectedAmount;

            // Assert
            Assert.Equal(expectedAmount, booking.TotalAmount);
        }

        [Fact]
        public void Booking_Status_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedStatus = "Confirmed";

            // Act
            booking.Status = expectedStatus;

            // Assert
            Assert.Equal(expectedStatus, booking.Status);
        }

        [Fact]
        public void Booking_Status_DefaultsTosPending()
        {
            // Arrange & Act
            var booking = new Booking();

            // Assert
            Assert.Equal("Pending", booking.Status);
        }

        [Fact]
        public void Booking_CreatedDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedDate = new DateTime(2024, 5, 1);

            // Act
            booking.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, booking.CreatedDate);
        }

        [Fact]
        public void Booking_ModifiedDate_CanBeNull()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.ModifiedDate = null;

            // Assert
            Assert.Null(booking.ModifiedDate);
        }

        [Fact]
        public void Booking_ModifiedDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedDate = new DateTime(2024, 5, 10);

            // Act
            booking.ModifiedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, booking.ModifiedDate);
        }

        [Fact]
        public void Booking_IsActive_CanBeSetToTrue()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.IsActive = true;

            // Assert
            Assert.True(booking.IsActive);
        }

        [Fact]
        public void Booking_IsActive_CanBeSetToFalse()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.IsActive = false;

            // Assert
            Assert.False(booking.IsActive);
        }

        [Fact]
        public void Booking_CreatedBy_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedCreator = "User123";

            // Act
            booking.CreatedBy = expectedCreator;

            // Assert
            Assert.Equal(expectedCreator, booking.CreatedBy);
        }

        [Fact]
        public void Booking_ModifiedBy_CanBeNull()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.ModifiedBy = null;

            // Assert
            Assert.Null(booking.ModifiedBy);
        }

        [Fact]
        public void Booking_ModifiedBy_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var expectedModifier = "Admin";

            // Act
            booking.ModifiedBy = expectedModifier;

            // Assert
            Assert.Equal(expectedModifier, booking.ModifiedBy);
        }

        [Fact]
        public void Booking_Tour_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var tour = new Tour { Id = 1, TourName = "Test Tour" };

            // Act
            booking.Tour = tour;

            // Assert
            Assert.NotNull(booking.Tour);
            Assert.Equal(tour, booking.Tour);
            Assert.Equal(1, booking.Tour.Id);
        }

        [Fact]
        public void Booking_User_CanBeSetAndRetrieved()
        {
            // Arrange
            var booking = new Booking();
            var user = new User { Id = 1, Email = "test@example.com" };

            // Act
            booking.User = user;

            // Assert
            Assert.NotNull(booking.User);
            Assert.Equal(user, booking.User);
            Assert.Equal(1, booking.User.Id);
        }

        [Fact]
        public void Booking_AllProperties_CanBeSetSimultaneously()
        {
            // Arrange
            var tour = new Tour { Id = 5, TourName = "European Tour" };
            var user = new User { Id = 10, Email = "customer@example.com" };
            var booking = new Booking
            {
                Id = 789,
                TourId = 5,
                UserId = 10,
                BookingDate = new DateTime(2024, 7, 1),
                NumberOfPersons = 2,
                TotalAmount = 5000.00m,
                Status = "Confirmed",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now.AddDays(1),
                IsActive = true,
                CreatedBy = "System",
                ModifiedBy = "Admin",
                Tour = tour,
                User = user
            };

            // Assert
            Assert.Equal(789, booking.Id);
            Assert.Equal(5, booking.TourId);
            Assert.Equal(10, booking.UserId);
            Assert.Equal(new DateTime(2024, 7, 1), booking.BookingDate);
            Assert.Equal(2, booking.NumberOfPersons);
            Assert.Equal(5000.00m, booking.TotalAmount);
            Assert.Equal("Confirmed", booking.Status);
            Assert.True(booking.IsActive);
            Assert.Equal("System", booking.CreatedBy);
            Assert.Equal("Admin", booking.ModifiedBy);
            Assert.NotNull(booking.Tour);
            Assert.NotNull(booking.User);
        }

        [Fact]
        public void Booking_NumberOfPersons_CanBeZero()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.NumberOfPersons = 0;

            // Assert
            Assert.Equal(0, booking.NumberOfPersons);
        }

        [Fact]
        public void Booking_TotalAmount_CanBeZero()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.TotalAmount = 0m;

            // Assert
            Assert.Equal(0m, booking.TotalAmount);
        }

        [Fact]
        public void Booking_Status_SupportsMultipleValues()
        {
            // Arrange
            var booking1 = new Booking { Status = "Pending" };
            var booking2 = new Booking { Status = "Confirmed" };
            var booking3 = new Booking { Status = "Cancelled" };

            // Assert
            Assert.Equal("Pending", booking1.Status);
            Assert.Equal("Confirmed", booking2.Status);
            Assert.Equal("Cancelled", booking3.Status);
        }
    }
}
