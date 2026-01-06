using System;
using TourManagement.Application.DTOs;
using Xunit;

namespace TourManagement.Application.DTOs.Tests
{
    public class TourDtoTests
    {
        [Fact]
        public void TourDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new TourDto();

            // Assert
            Assert.Equal(0, dto.Id);
            Assert.Equal(string.Empty, dto.TourName);
            Assert.Equal(string.Empty, dto.Place);
            Assert.Equal(0, dto.Days);
            Assert.Equal(0m, dto.Price);
            Assert.Equal(string.Empty, dto.Locations);
            Assert.Equal(string.Empty, dto.TourInfo);
            Assert.Null(dto.PicturePath);
            Assert.False(dto.IsActive);
        }

        [Fact]
        public void TourDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new TourDto();

            // Act
            dto.Id = 1;
            dto.TourName = "Paris Tour";
            dto.Place = "France";
            dto.Days = 7;
            dto.Price = 1500.50m;
            dto.Locations = "Paris, Lyon";
            dto.TourInfo = "Amazing tour";
            dto.PicturePath = "/images/tour.jpg";
            dto.IsActive = true;

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("Paris Tour", dto.TourName);
            Assert.Equal("France", dto.Place);
            Assert.Equal(7, dto.Days);
            Assert.Equal(1500.50m, dto.Price);
            Assert.Equal("Paris, Lyon", dto.Locations);
            Assert.Equal("Amazing tour", dto.TourInfo);
            Assert.Equal("/images/tour.jpg", dto.PicturePath);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void TourCreateDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new TourCreateDto();

            // Assert
            Assert.Equal(string.Empty, dto.TourName);
            Assert.Equal(string.Empty, dto.Place);
            Assert.Equal(0, dto.Days);
            Assert.Equal(0m, dto.Price);
            Assert.Equal(string.Empty, dto.Locations);
            Assert.Equal(string.Empty, dto.TourInfo);
            Assert.Null(dto.PicturePath);
        }

        [Fact]
        public void TourCreateDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new TourCreateDto();

            // Act
            dto.TourName = "New Tour";
            dto.Place = "Italy";
            dto.Days = 5;
            dto.Price = 1200m;
            dto.Locations = "Rome, Milan";
            dto.TourInfo = "Fantastic tour";
            dto.PicturePath = "/images/new.jpg";

            // Assert
            Assert.Equal("New Tour", dto.TourName);
            Assert.Equal("Italy", dto.Place);
            Assert.Equal(5, dto.Days);
            Assert.Equal(1200m, dto.Price);
            Assert.Equal("Rome, Milan", dto.Locations);
            Assert.Equal("Fantastic tour", dto.TourInfo);
            Assert.Equal("/images/new.jpg", dto.PicturePath);
        }

        [Fact]
        public void TourUpdateDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new TourUpdateDto();

            // Assert
            Assert.Equal(string.Empty, dto.TourName);
            Assert.Equal(string.Empty, dto.Place);
            Assert.Equal(0, dto.Days);
            Assert.Equal(0m, dto.Price);
            Assert.Equal(string.Empty, dto.Locations);
            Assert.Equal(string.Empty, dto.TourInfo);
            Assert.Null(dto.PicturePath);
            Assert.False(dto.IsActive);
        }

        [Fact]
        public void TourUpdateDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new TourUpdateDto();

            // Act
            dto.TourName = "Updated Tour";
            dto.Place = "Spain";
            dto.Days = 10;
            dto.Price = 2000m;
            dto.Locations = "Madrid, Barcelona";
            dto.TourInfo = "Great tour";
            dto.PicturePath = "/images/updated.jpg";
            dto.IsActive = true;

            // Assert
            Assert.Equal("Updated Tour", dto.TourName);
            Assert.Equal("Spain", dto.Place);
            Assert.Equal(10, dto.Days);
            Assert.Equal(2000m, dto.Price);
            Assert.Equal("Madrid, Barcelona", dto.Locations);
            Assert.Equal("Great tour", dto.TourInfo);
            Assert.Equal("/images/updated.jpg", dto.PicturePath);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void TourDto_Price_ShouldAcceptDecimalValues()
        {
            // Arrange
            var dto = new TourDto();

            // Act
            dto.Price = 1234.56m;

            // Assert
            Assert.Equal(1234.56m, dto.Price);
        }

        [Fact]
        public void TourDto_Days_ShouldAcceptNegativeValue()
        {
            // Arrange
            var dto = new TourDto();

            // Act
            dto.Days = -1;

            // Assert
            Assert.Equal(-1, dto.Days);
        }

        [Fact]
        public void TourDto_PicturePath_ShouldAcceptNull()
        {
            // Arrange
            var dto = new TourDto();

            // Act
            dto.PicturePath = null;

            // Assert
            Assert.Null(dto.PicturePath);
        }

        [Fact]
        public void TourCreateDto_PicturePath_ShouldAcceptNull()
        {
            // Arrange
            var dto = new TourCreateDto();

            // Act
            dto.PicturePath = null;

            // Assert
            Assert.Null(dto.PicturePath);
        }

        [Fact]
        public void TourUpdateDto_PicturePath_ShouldAcceptNull()
        {
            // Arrange
            var dto = new TourUpdateDto();

            // Act
            dto.PicturePath = null;

            // Assert
            Assert.Null(dto.PicturePath);
        }
    }
}
