using System;
using System.Collections.Generic;
using TourManagement.Domain.Entities;
using Xunit;

namespace TourManagement.Domain.Entities.Tests
{
    public class UserTests
    {
        [Fact]
        public void User_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Equal(0, user.Id);
            Assert.Equal(string.Empty, user.Email);
            Assert.Equal(string.Empty, user.PasswordHash);
            Assert.Equal(string.Empty, user.FullName);
            Assert.Null(user.PhoneNumber);
            Assert.Null(user.Address);
            Assert.Equal(default(DateTime), user.CreatedDate);
            Assert.Null(user.ModifiedDate);
            Assert.False(user.IsActive);
            Assert.Equal(string.Empty, user.CreatedBy);
            Assert.Null(user.ModifiedBy);
            Assert.NotNull(user.Bookings);
            Assert.Empty(user.Bookings);
        }

        [Fact]
        public void User_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var user = new User();
            var createdDate = DateTime.Now;
            var modifiedDate = DateTime.Now.AddDays(1);

            // Act
            user.Id = 1;
            user.Email = "test@example.com";
            user.PasswordHash = "hashed_password";
            user.FullName = "John Doe";
            user.PhoneNumber = "1234567890";
            user.Address = "123 Main St";
            user.CreatedDate = createdDate;
            user.ModifiedDate = modifiedDate;
            user.IsActive = true;
            user.CreatedBy = "admin";
            user.ModifiedBy = "admin2";

            // Assert
            Assert.Equal(1, user.Id);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("hashed_password", user.PasswordHash);
            Assert.Equal("John Doe", user.FullName);
            Assert.Equal("1234567890", user.PhoneNumber);
            Assert.Equal("123 Main St", user.Address);
            Assert.Equal(createdDate, user.CreatedDate);
            Assert.Equal(modifiedDate, user.ModifiedDate);
            Assert.True(user.IsActive);
            Assert.Equal("admin", user.CreatedBy);
            Assert.Equal("admin2", user.ModifiedBy);
        }

        [Fact]
        public void User_Email_ShouldAcceptValidEmail()
        {
            // Arrange
            var user = new User();

            // Act
            user.Email = "valid.email@domain.com";

            // Assert
            Assert.Equal("valid.email@domain.com", user.Email);
        }

        [Fact]
        public void User_Email_ShouldAcceptEmptyString()
        {
            // Arrange
            var user = new User();

            // Act
            user.Email = "";

            // Assert
            Assert.Equal("", user.Email);
        }

        [Fact]
        public void User_PasswordHash_ShouldStoreHashedValue()
        {
            // Arrange
            var user = new User();
            var hashedPassword = "SHA256_HASHED_VALUE";

            // Act
            user.PasswordHash = hashedPassword;

            // Assert
            Assert.Equal(hashedPassword, user.PasswordHash);
        }

        [Fact]
        public void User_PhoneNumber_ShouldAcceptNull()
        {
            // Arrange
            var user = new User();

            // Act
            user.PhoneNumber = null;

            // Assert
            Assert.Null(user.PhoneNumber);
        }

        [Fact]
        public void User_Address_ShouldAcceptNull()
        {
            // Arrange
            var user = new User();

            // Act
            user.Address = null;

            // Assert
            Assert.Null(user.Address);
        }

        [Fact]
        public void User_ModifiedBy_ShouldAcceptNull()
        {
            // Arrange
            var user = new User();

            // Act
            user.ModifiedBy = null;

            // Assert
            Assert.Null(user.ModifiedBy);
        }

        [Fact]
        public void User_ModifiedDate_ShouldAcceptNull()
        {
            // Arrange
            var user = new User();

            // Act
            user.ModifiedDate = null;

            // Assert
            Assert.Null(user.ModifiedDate);
        }

        [Fact]
        public void User_IsActive_ShouldToggle()
        {
            // Arrange
            var user = new User { IsActive = false };

            // Act
            user.IsActive = true;

            // Assert
            Assert.True(user.IsActive);

            // Act
            user.IsActive = false;

            // Assert
            Assert.False(user.IsActive);
        }

        [Fact]
        public void User_Bookings_ShouldInitializeAsEmptyList()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user.Bookings);
            Assert.IsType<List<Booking>>(user.Bookings);
            Assert.Empty(user.Bookings);
        }

        [Fact]
        public void User_Bookings_ShouldAllowAddingItems()
        {
            // Arrange
            var user = new User();
            var booking = new Booking { Id = 1, UserId = user.Id };

            // Act
            user.Bookings.Add(booking);

            // Assert
            Assert.Single(user.Bookings);
            Assert.Contains(booking, user.Bookings);
        }

        [Fact]
        public void User_FullName_ShouldAcceptLongNames()
        {
            // Arrange
            var user = new User();
            var longName = "Alexander Benjamin Christopher Davidson Emmanuel";

            // Act
            user.FullName = longName;

            // Assert
            Assert.Equal(longName, user.FullName);
        }

        [Fact]
        public void User_AllStringProperties_ShouldHandleEmptyStrings()
        {
            // Arrange
            var user = new User();

            // Act
            user.Email = "";
            user.PasswordHash = "";
            user.FullName = "";
            user.CreatedBy = "";

            // Assert
            Assert.Equal("", user.Email);
            Assert.Equal("", user.PasswordHash);
            Assert.Equal("", user.FullName);
            Assert.Equal("", user.CreatedBy);
        }

        [Fact]
        public void User_CreatedDate_ShouldStoreDateTime()
        {
            // Arrange
            var user = new User();
            var date = new DateTime(2023, 1, 15, 10, 30, 0);

            // Act
            user.CreatedDate = date;

            // Assert
            Assert.Equal(date, user.CreatedDate);
        }
    }
}
