using System;
using TourManagement.Application.DTOs;
using Xunit;

namespace TourManagement.Application.DTOs.Tests
{
    public class BookingDtoTests
    {
        [Fact]
        public void BookingDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new BookingDto();

            // Assert
            Assert.Equal(0, dto.Id);
            Assert.Equal(0, dto.UserId);
            Assert.Equal(0, dto.TourId);
            Assert.Equal(default(DateTime), dto.BookingDate);
            Assert.Equal(0, dto.NumberOfPeople);
            Assert.Equal(0m, dto.TotalAmount);
            Assert.Equal(string.Empty, dto.Status);
            Assert.Null(dto.Notes);
            Assert.Equal(string.Empty, dto.UserName);
            Assert.Equal(string.Empty, dto.TourName);
        }

        [Fact]
        public void BookingDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new BookingDto();
            var bookingDate = DateTime.Now;

            // Act
            dto.Id = 1;
            dto.UserId = 10;
            dto.TourId = 20;
            dto.BookingDate = bookingDate;
            dto.NumberOfPeople = 4;
            dto.TotalAmount = 5000.50m;
            dto.Status = "Confirmed";
            dto.Notes = "Special requests";
            dto.UserName = "John Doe";
            dto.TourName = "Paris Tour";

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal(10, dto.UserId);
            Assert.Equal(20, dto.TourId);
            Assert.Equal(bookingDate, dto.BookingDate);
            Assert.Equal(4, dto.NumberOfPeople);
            Assert.Equal(5000.50m, dto.TotalAmount);
            Assert.Equal("Confirmed", dto.Status);
            Assert.Equal("Special requests", dto.Notes);
            Assert.Equal("John Doe", dto.UserName);
            Assert.Equal("Paris Tour", dto.TourName);
        }

        [Fact]
        public void BookingCreateDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new BookingCreateDto();

            // Assert
            Assert.Equal(0, dto.UserId);
            Assert.Equal(0, dto.TourId);
            Assert.Equal(default(DateTime), dto.BookingDate);
            Assert.Equal(0, dto.NumberOfPeople);
            Assert.Equal(0m, dto.TotalAmount);
            Assert.Null(dto.Notes);
        }

        [Fact]
        public void BookingCreateDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new BookingCreateDto();
            var bookingDate = DateTime.Now;

            // Act
            dto.UserId = 5;
            dto.TourId = 15;
            dto.BookingDate = bookingDate;
            dto.NumberOfPeople = 2;
            dto.TotalAmount = 3000m;
            dto.Notes = "Early check-in";

            // Assert
            Assert.Equal(5, dto.UserId);
            Assert.Equal(15, dto.TourId);
            Assert.Equal(bookingDate, dto.BookingDate);
            Assert.Equal(2, dto.NumberOfPeople);
            Assert.Equal(3000m, dto.TotalAmount);
            Assert.Equal("Early check-in", dto.Notes);
        }

        [Fact]
        public void BookingUpdateDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new BookingUpdateDto();

            // Assert
            Assert.Equal(default(DateTime), dto.BookingDate);
            Assert.Equal(0, dto.NumberOfPeople);
            Assert.Equal(0m, dto.TotalAmount);
            Assert.Equal(string.Empty, dto.Status);
            Assert.Null(dto.Notes);
        }

        [Fact]
        public void BookingUpdateDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new BookingUpdateDto();
            var bookingDate = DateTime.Now;

            // Act
            dto.BookingDate = bookingDate;
            dto.NumberOfPeople = 6;
            dto.TotalAmount = 7500m;
            dto.Status = "Pending";
            dto.Notes = "Updated notes";

            // Assert
            Assert.Equal(bookingDate, dto.BookingDate);
            Assert.Equal(6, dto.NumberOfPeople);
            Assert.Equal(7500m, dto.TotalAmount);
            Assert.Equal("Pending", dto.Status);
            Assert.Equal("Updated notes", dto.Notes);
        }

        [Fact]
        public void BookingDto_Notes_ShouldAcceptNull()
        {
            // Arrange
            var dto = new BookingDto();

            // Act
            dto.Notes = null;

            // Assert
            Assert.Null(dto.Notes);
        }

        [Fact]
        public void BookingCreateDto_Notes_ShouldAcceptNull()
        {
            // Arrange
            var dto = new BookingCreateDto();

            // Act
            dto.Notes = null;

            // Assert
            Assert.Null(dto.Notes);
        }

        [Fact]
        public void BookingUpdateDto_Notes_ShouldAcceptNull()
        {
            // Arrange
            var dto = new BookingUpdateDto();

            // Act
            dto.Notes = null;

            // Assert
            Assert.Null(dto.Notes);
        }

        [Fact]
        public void BookingDto_TotalAmount_ShouldAcceptDecimalValue()
        {
            // Arrange
            var dto = new BookingDto();

            // Act
            dto.TotalAmount = 1234.56m;

            // Assert
            Assert.Equal(1234.56m, dto.TotalAmount);
        }

        [Fact]
        public void BookingDto_NumberOfPeople_ShouldAcceptPositiveValue()
        {
            // Arrange
            var dto = new BookingDto();

            // Act
            dto.NumberOfPeople = 10;

            // Assert
            Assert.Equal(10, dto.NumberOfPeople);
        }

        [Fact]
        public void BookingDto_Status_ShouldAcceptDifferentStatuses()
        {
            // Arrange
            var dto = new BookingDto();

            // Act & Assert
            dto.Status = "Pending";
            Assert.Equal("Pending", dto.Status);

            dto.Status = "Confirmed";
            Assert.Equal("Confirmed", dto.Status);

            dto.Status = "Cancelled";
            Assert.Equal("Cancelled", dto.Status);
        }
    }
}
