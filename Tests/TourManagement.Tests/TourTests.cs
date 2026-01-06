using System;
using System.Collections.Generic;
using TourManagement.Domain.Entities;
using Xunit;

namespace TourManagement.Domain.Entities.Tests
{
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
            Assert.Equal(0m, tour.Price);
            Assert.Equal(string.Empty, tour.Locations);
            Assert.Equal(string.Empty, tour.TourInfo);
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
        public void Tour_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var tour = new Tour();
            var createdDate = DateTime.Now;
            var modifiedDate = DateTime.Now.AddDays(1);

            // Act
            tour.Id = 1;
            tour.TourName = "Paris Tour";
            tour.Place = "France";
            tour.Days = 7;
            tour.Price = 1500.50m;
            tour.Locations = "Paris, Lyon, Nice";
            tour.TourInfo = "Amazing tour of France";
            tour.PicturePath = "/images/tour1.jpg";
            tour.CreatedDate = createdDate;
            tour.ModifiedDate = modifiedDate;
            tour.IsActive = true;
            tour.CreatedBy = "admin";
            tour.ModifiedBy = "admin2";

            // Assert
            Assert.Equal(1, tour.Id);
            Assert.Equal("Paris Tour", tour.TourName);
            Assert.Equal("France", tour.Place);
            Assert.Equal(7, tour.Days);
            Assert.Equal(1500.50m, tour.Price);
            Assert.Equal("Paris, Lyon, Nice", tour.Locations);
            Assert.Equal("Amazing tour of France", tour.TourInfo);
            Assert.Equal("/images/tour1.jpg", tour.PicturePath);
            Assert.Equal(createdDate, tour.CreatedDate);
            Assert.Equal(modifiedDate, tour.ModifiedDate);
            Assert.True(tour.IsActive);
            Assert.Equal("admin", tour.CreatedBy);
            Assert.Equal("admin2", tour.ModifiedBy);
        }

        [Fact]
        public void Tour_Price_ShouldAcceptZero()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.Price = 0m;

            // Assert
            Assert.Equal(0m, tour.Price);
        }

        [Fact]
        public void Tour_Price_ShouldAcceptNegativeValue()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.Price = -100m;

            // Assert
            Assert.Equal(-100m, tour.Price);
        }

        [Fact]
        public void Tour_Days_ShouldAcceptPositiveValue()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.Days = 10;

            // Assert
            Assert.Equal(10, tour.Days);
        }

        [Fact]
        public void Tour_Days_ShouldAcceptNegativeValue()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.Days = -5;

            // Assert
            Assert.Equal(-5, tour.Days);
        }

        [Fact]
        public void Tour_Bookings_ShouldInitializeAsEmptyList()
        {
            // Arrange & Act
            var tour = new Tour();

            // Assert
            Assert.NotNull(tour.Bookings);
            Assert.IsType<List<Booking>>(tour.Bookings);
            Assert.Empty(tour.Bookings);
        }

        [Fact]
        public void Tour_Bookings_ShouldAllowAddingItems()
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
        public void Tour_PicturePath_ShouldAcceptNull()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.PicturePath = null;

            // Assert
            Assert.Null(tour.PicturePath);
        }

        [Fact]
        public void Tour_ModifiedBy_ShouldAcceptNull()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.ModifiedBy = null;

            // Assert
            Assert.Null(tour.ModifiedBy);
        }

        [Fact]
        public void Tour_ModifiedDate_ShouldAcceptNull()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.ModifiedDate = null;

            // Assert
            Assert.Null(tour.ModifiedDate);
        }

        [Fact]
        public void Tour_IsActive_ShouldToggle()
        {
            // Arrange
            var tour = new Tour { IsActive = false };

            // Act
            tour.IsActive = true;

            // Assert
            Assert.True(tour.IsActive);

            // Act
            tour.IsActive = false;

            // Assert
            Assert.False(tour.IsActive);
        }

        [Fact]
        public void Tour_AllStringProperties_ShouldHandleEmptyStrings()
        {
            // Arrange
            var tour = new Tour();

            // Act
            tour.TourName = "";
            tour.Place = "";
            tour.Locations = "";
            tour.TourInfo = "";
            tour.CreatedBy = "";

            // Assert
            Assert.Equal("", tour.TourName);
            Assert.Equal("", tour.Place);
            Assert.Equal("", tour.Locations);
            Assert.Equal("", tour.TourInfo);
            Assert.Equal("", tour.CreatedBy);
        }
    }
}
