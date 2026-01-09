using System;
using System.Collections.Generic;
using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Entities.Tests
{
    public class TourTests
    {
        [Fact]
        public void Tour_Constructor_InitializesWithDefaultValues()
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
            Assert.Equal(string.Empty, tour.TourInfo);
            Assert.Null(tour.PicturePath);
            Assert.Equal(default(DateTime), tour.CreatedDate);
            Assert.Null(tour.ModifiedDate);
            Assert.False(tour.IsActive);
            Assert.Equal("System", tour.CreatedBy);
            Assert.Null(tour.ModifiedBy);
            Assert.NotNull(tour.Bookings);
            Assert.Empty(tour.Bookings);
        }

        [Fact]
        public void Tour_Id_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedId = 42;

            // Act
            tour.Id = expectedId;

            // Assert
            Assert.Equal(expectedId, tour.Id);
        }

        [Fact]
        public void Tour_TourName_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedName = "European Adventure";

            // Act
            tour.TourName = expectedName;

            // Assert
            Assert.Equal(expectedName, tour.TourName);
        }

        [Fact]
        public void Tour_Place_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedPlace = "Paris";

            // Act
            tour.Place = expectedPlace;

            // Assert
            Assert.Equal(expectedPlace, tour.Place);
        }

        [Fact]
        public void Tour_Days_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedDays = 7;

            // Act
            tour.Days = expectedDays;

            // Assert
            Assert.Equal(expectedDays, tour.Days);
        }

        [Fact]
        public void Tour_Price_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedPrice = 1500.50m;

            // Act
            tour.Price = expectedPrice;

            // Assert
            Assert.Equal(expectedPrice, tour.Price);
        }

        [Fact]
        public void Tour_Locations_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedLocations = "Paris, London, Rome";

            // Act
            tour.Locations = expectedLocations;

            // Assert
            Assert.Equal(expectedLocations, tour.Locations);
        }

        [Fact]
        public void Tour_TourInfo_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedInfo = "A wonderful tour across Europe";

            // Act
            tour.TourInfo = expectedInfo;

            // Assert
            Assert.Equal(expectedInfo, tour.TourInfo);
        }

        [Fact]
        public void Tour_PicturePath_CanBeNull()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.PicturePath = null;

            // Assert
            Assert.Null(tour.PicturePath);
        }

        [Fact]
        public void Tour_PicturePath_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedPath = "/images/tour1.jpg";

            // Act
            tour.PicturePath = expectedPath;

            // Assert
            Assert.Equal(expectedPath, tour.PicturePath);
        }

        [Fact]
        public void Tour_CreatedDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedDate = new DateTime(2024, 1, 15);

            // Act
            tour.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, tour.CreatedDate);
        }

        [Fact]
        public void Tour_ModifiedDate_CanBeNull()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.ModifiedDate = null;

            // Assert
            Assert.Null(tour.ModifiedDate);
        }

        [Fact]
        public void Tour_ModifiedDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedDate = new DateTime(2024, 2, 20);

            // Act
            tour.ModifiedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, tour.ModifiedDate);
        }

        [Fact]
        public void Tour_IsActive_CanBeSetToTrue()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.IsActive = true;

            // Assert
            Assert.True(tour.IsActive);
        }

        [Fact]
        public void Tour_IsActive_CanBeSetToFalse()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.IsActive = false;

            // Assert
            Assert.False(tour.IsActive);
        }

        [Fact]
        public void Tour_CreatedBy_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedCreator = "Admin";

            // Act
            tour.CreatedBy = expectedCreator;

            // Assert
            Assert.Equal(expectedCreator, tour.CreatedBy);
        }

        [Fact]
        public void Tour_ModifiedBy_CanBeNull()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.ModifiedBy = null;

            // Assert
            Assert.Null(tour.ModifiedBy);
        }

        [Fact]
        public void Tour_ModifiedBy_CanBeSetAndRetrieved()
        {
            // Arrange
            var tour = new Tour();
            var expectedModifier = "Editor";

            // Act
            tour.ModifiedBy = expectedModifier;

            // Assert
            Assert.Equal(expectedModifier, tour.ModifiedBy);
        }

        [Fact]
        public void Tour_Bookings_InitializesAsEmptyCollection()
        {
            // Arrange
            var tour = new Tour();

            // Act & Assert
            Assert.NotNull(tour.Bookings);
            Assert.Empty(tour.Bookings);
        }

        [Fact]
        public void Tour_Bookings_CanAddBooking()
        {
            // Arrange
            var tour = new Tour();
            var booking = new Booking { Id = 1, TourId = 1 };

            // Act
            tour.Bookings.Add(booking);

            // Assert
            Assert.Single(tour.Bookings);
            Assert.Contains(booking, tour.Bookings);
        }

        [Fact]
        public void Tour_Bookings_CanAddMultipleBookings()
        {
            // Arrange
            var tour = new Tour();
            var booking1 = new Booking { Id = 1, TourId = 1 };
            var booking2 = new Booking { Id = 2, TourId = 1 };

            // Act
            tour.Bookings.Add(booking1);
            tour.Bookings.Add(booking2);

            // Assert
            Assert.Equal(2, tour.Bookings.Count);
            Assert.Contains(booking1, tour.Bookings);
            Assert.Contains(booking2, tour.Bookings);
        }

        [Fact]
        public void Tour_Price_CanBeNegative()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.Price = -100.00m;

            // Assert
            Assert.Equal(-100.00m, tour.Price);
        }

        [Fact]
        public void Tour_Days_CanBeNegative()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.Days = -5;

            // Assert
            Assert.Equal(-5, tour.Days);
        }

        [Fact]
        public void Tour_AllProperties_CanBeSetSimultaneously()
        {
            // Arrange
            var tour = new Tour
            {
                Id = 100,
                TourName = "Asia Explorer",
                Place = "Tokyo",
                Days = 10,
                Price = 2500.75m,
                Locations = "Tokyo, Kyoto, Osaka",
                TourInfo = "Experience the beauty of Japan",
                PicturePath = "/images/asia.jpg",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now.AddDays(1),
                IsActive = true,
                CreatedBy = "Admin",
                ModifiedBy = "Moderator"
            };

            // Assert
            Assert.Equal(100, tour.Id);
            Assert.Equal("Asia Explorer", tour.TourName);
            Assert.Equal("Tokyo", tour.Place);
            Assert.Equal(10, tour.Days);
            Assert.Equal(2500.75m, tour.Price);
            Assert.Equal("Tokyo, Kyoto, Osaka", tour.Locations);
            Assert.Equal("Experience the beauty of Japan", tour.TourInfo);
            Assert.Equal("/images/asia.jpg", tour.PicturePath);
            Assert.True(tour.IsActive);
            Assert.Equal("Admin", tour.CreatedBy);
            Assert.Equal("Moderator", tour.ModifiedBy);
        }
    }
}
