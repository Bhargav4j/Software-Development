using System;
using System.Collections.Generic;
using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Entities.Tests
{
    public class UserTests
    {
        [Fact]
        public void User_Constructor_InitializesWithDefaultValues()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Equal(0, user.Id);
            Assert.Equal(string.Empty, user.Email);
            Assert.Equal(string.Empty, user.FirstName);
            Assert.Equal(string.Empty, user.LastName);
            Assert.Equal(string.Empty, user.Gender);
            Assert.Equal(string.Empty, user.PasswordHash);
            Assert.Null(user.DateOfBirth);
            Assert.Equal(string.Empty, user.Street);
            Assert.Equal(string.Empty, user.City);
            Assert.Equal(string.Empty, user.State);
            Assert.Equal(default(DateTime), user.CreatedDate);
            Assert.Null(user.ModifiedDate);
            Assert.False(user.IsActive);
            Assert.Equal("System", user.CreatedBy);
            Assert.Null(user.ModifiedBy);
            Assert.NotNull(user.Bookings);
            Assert.Empty(user.Bookings);
        }

        [Fact]
        public void User_Id_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedId = 123;

            // Act
            user.Id = expectedId;

            // Assert
            Assert.Equal(expectedId, user.Id);
        }

        [Fact]
        public void User_Email_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedEmail = "test@example.com";

            // Act
            user.Email = expectedEmail;

            // Assert
            Assert.Equal(expectedEmail, user.Email);
        }

        [Fact]
        public void User_FirstName_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedFirstName = "John";

            // Act
            user.FirstName = expectedFirstName;

            // Assert
            Assert.Equal(expectedFirstName, user.FirstName);
        }

        [Fact]
        public void User_LastName_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedLastName = "Doe";

            // Act
            user.LastName = expectedLastName;

            // Assert
            Assert.Equal(expectedLastName, user.LastName);
        }

        [Fact]
        public void User_Gender_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedGender = "Male";

            // Act
            user.Gender = expectedGender;

            // Assert
            Assert.Equal(expectedGender, user.Gender);
        }

        [Fact]
        public void User_PasswordHash_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedHash = "hashed_password_12345";

            // Act
            user.PasswordHash = expectedHash;

            // Assert
            Assert.Equal(expectedHash, user.PasswordHash);
        }

        [Fact]
        public void User_DateOfBirth_CanBeNull()
        {
            // Arrange
            var user = new User();

            // Act
            user.DateOfBirth = null;

            // Assert
            Assert.Null(user.DateOfBirth);
        }

        [Fact]
        public void User_DateOfBirth_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedDate = new DateTime(1990, 5, 15);

            // Act
            user.DateOfBirth = expectedDate;

            // Assert
            Assert.Equal(expectedDate, user.DateOfBirth);
        }

        [Fact]
        public void User_Street_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedStreet = "123 Main Street";

            // Act
            user.Street = expectedStreet;

            // Assert
            Assert.Equal(expectedStreet, user.Street);
        }

        [Fact]
        public void User_City_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedCity = "New York";

            // Act
            user.City = expectedCity;

            // Assert
            Assert.Equal(expectedCity, user.City);
        }

        [Fact]
        public void User_State_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedState = "NY";

            // Act
            user.State = expectedState;

            // Assert
            Assert.Equal(expectedState, user.State);
        }

        [Fact]
        public void User_CreatedDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedDate = new DateTime(2024, 1, 1);

            // Act
            user.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, user.CreatedDate);
        }

        [Fact]
        public void User_ModifiedDate_CanBeNull()
        {
            // Arrange
            var user = new User();

            // Act
            user.ModifiedDate = null;

            // Assert
            Assert.Null(user.ModifiedDate);
        }

        [Fact]
        public void User_ModifiedDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedDate = new DateTime(2024, 2, 1);

            // Act
            user.ModifiedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, user.ModifiedDate);
        }

        [Fact]
        public void User_IsActive_CanBeSetToTrue()
        {
            // Arrange
            var user = new User();

            // Act
            user.IsActive = true;

            // Assert
            Assert.True(user.IsActive);
        }

        [Fact]
        public void User_IsActive_CanBeSetToFalse()
        {
            // Arrange
            var user = new User();

            // Act
            user.IsActive = false;

            // Assert
            Assert.False(user.IsActive);
        }

        [Fact]
        public void User_CreatedBy_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedCreator = "Admin";

            // Act
            user.CreatedBy = expectedCreator;

            // Assert
            Assert.Equal(expectedCreator, user.CreatedBy);
        }

        [Fact]
        public void User_ModifiedBy_CanBeNull()
        {
            // Arrange
            var user = new User();

            // Act
            user.ModifiedBy = null;

            // Assert
            Assert.Null(user.ModifiedBy);
        }

        [Fact]
        public void User_ModifiedBy_CanBeSetAndRetrieved()
        {
            // Arrange
            var user = new User();
            var expectedModifier = "Editor";

            // Act
            user.ModifiedBy = expectedModifier;

            // Assert
            Assert.Equal(expectedModifier, user.ModifiedBy);
        }

        [Fact]
        public void User_Bookings_InitializesAsEmptyCollection()
        {
            // Arrange
            var user = new User();

            // Act & Assert
            Assert.NotNull(user.Bookings);
            Assert.Empty(user.Bookings);
        }

        [Fact]
        public void User_Bookings_CanAddBooking()
        {
            // Arrange
            var user = new User();
            var booking = new Booking { Id = 1, UserId = 1 };

            // Act
            user.Bookings.Add(booking);

            // Assert
            Assert.Single(user.Bookings);
            Assert.Contains(booking, user.Bookings);
        }

        [Fact]
        public void User_Bookings_CanAddMultipleBookings()
        {
            // Arrange
            var user = new User();
            var booking1 = new Booking { Id = 1, UserId = 1 };
            var booking2 = new Booking { Id = 2, UserId = 1 };

            // Act
            user.Bookings.Add(booking1);
            user.Bookings.Add(booking2);

            // Assert
            Assert.Equal(2, user.Bookings.Count);
            Assert.Contains(booking1, user.Bookings);
            Assert.Contains(booking2, user.Bookings);
        }

        [Fact]
        public void User_AllProperties_CanBeSetSimultaneously()
        {
            // Arrange
            var user = new User
            {
                Id = 99,
                Email = "jane@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Gender = "Female",
                PasswordHash = "hashed_password",
                DateOfBirth = new DateTime(1985, 3, 20),
                Street = "456 Oak Avenue",
                City = "Los Angeles",
                State = "CA",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now.AddDays(1),
                IsActive = true,
                CreatedBy = "System",
                ModifiedBy = "Admin"
            };

            // Assert
            Assert.Equal(99, user.Id);
            Assert.Equal("jane@example.com", user.Email);
            Assert.Equal("Jane", user.FirstName);
            Assert.Equal("Smith", user.LastName);
            Assert.Equal("Female", user.Gender);
            Assert.Equal("hashed_password", user.PasswordHash);
            Assert.Equal(new DateTime(1985, 3, 20), user.DateOfBirth);
            Assert.Equal("456 Oak Avenue", user.Street);
            Assert.Equal("Los Angeles", user.City);
            Assert.Equal("CA", user.State);
            Assert.True(user.IsActive);
            Assert.Equal("System", user.CreatedBy);
            Assert.Equal("Admin", user.ModifiedBy);
        }

        [Fact]
        public void User_Email_CanBeEmpty()
        {
            // Arrange
            var user = new User();

            // Act
            user.Email = string.Empty;

            // Assert
            Assert.Equal(string.Empty, user.Email);
        }

        [Fact]
        public void User_Gender_SupportsMultipleValues()
        {
            // Arrange
            var user1 = new User { Gender = "Male" };
            var user2 = new User { Gender = "Female" };
            var user3 = new User { Gender = "Other" };

            // Assert
            Assert.Equal("Male", user1.Gender);
            Assert.Equal("Female", user2.Gender);
            Assert.Equal("Other", user3.Gender);
        }
    }
}
