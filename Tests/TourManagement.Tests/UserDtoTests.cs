using System;
using TourManagement.Application.DTOs;
using Xunit;

namespace TourManagement.Application.DTOs.Tests
{
    public class UserDtoTests
    {
        [Fact]
        public void UserDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new UserDto();

            // Assert
            Assert.Equal(0, dto.Id);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FullName);
            Assert.Null(dto.PhoneNumber);
            Assert.Null(dto.Address);
            Assert.False(dto.IsActive);
        }

        [Fact]
        public void UserDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new UserDto();

            // Act
            dto.Id = 1;
            dto.Email = "test@example.com";
            dto.FullName = "John Doe";
            dto.PhoneNumber = "1234567890";
            dto.Address = "123 Main St";
            dto.IsActive = true;

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("test@example.com", dto.Email);
            Assert.Equal("John Doe", dto.FullName);
            Assert.Equal("1234567890", dto.PhoneNumber);
            Assert.Equal("123 Main St", dto.Address);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void UserCreateDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new UserCreateDto();

            // Assert
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.Password);
            Assert.Equal(string.Empty, dto.FullName);
            Assert.Null(dto.PhoneNumber);
            Assert.Null(dto.Address);
        }

        [Fact]
        public void UserCreateDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new UserCreateDto();

            // Act
            dto.Email = "newuser@example.com";
            dto.Password = "SecurePassword123";
            dto.FullName = "Jane Smith";
            dto.PhoneNumber = "9876543210";
            dto.Address = "456 Oak Ave";

            // Assert
            Assert.Equal("newuser@example.com", dto.Email);
            Assert.Equal("SecurePassword123", dto.Password);
            Assert.Equal("Jane Smith", dto.FullName);
            Assert.Equal("9876543210", dto.PhoneNumber);
            Assert.Equal("456 Oak Ave", dto.Address);
        }

        [Fact]
        public void UserUpdateDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new UserUpdateDto();

            // Assert
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FullName);
            Assert.Null(dto.PhoneNumber);
            Assert.Null(dto.Address);
            Assert.False(dto.IsActive);
        }

        [Fact]
        public void UserUpdateDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new UserUpdateDto();

            // Act
            dto.Email = "updated@example.com";
            dto.FullName = "Updated Name";
            dto.PhoneNumber = "5555555555";
            dto.Address = "789 Pine Rd";
            dto.IsActive = true;

            // Assert
            Assert.Equal("updated@example.com", dto.Email);
            Assert.Equal("Updated Name", dto.FullName);
            Assert.Equal("5555555555", dto.PhoneNumber);
            Assert.Equal("789 Pine Rd", dto.Address);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void UserLoginDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new UserLoginDto();

            // Assert
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.Password);
        }

        [Fact]
        public void UserLoginDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new UserLoginDto();

            // Act
            dto.Email = "login@example.com";
            dto.Password = "MyPassword123";

            // Assert
            Assert.Equal("login@example.com", dto.Email);
            Assert.Equal("MyPassword123", dto.Password);
        }

        [Fact]
        public void UserDto_PhoneNumber_ShouldAcceptNull()
        {
            // Arrange
            var dto = new UserDto();

            // Act
            dto.PhoneNumber = null;

            // Assert
            Assert.Null(dto.PhoneNumber);
        }

        [Fact]
        public void UserDto_Address_ShouldAcceptNull()
        {
            // Arrange
            var dto = new UserDto();

            // Act
            dto.Address = null;

            // Assert
            Assert.Null(dto.Address);
        }

        [Fact]
        public void UserCreateDto_PhoneNumber_ShouldAcceptNull()
        {
            // Arrange
            var dto = new UserCreateDto();

            // Act
            dto.PhoneNumber = null;

            // Assert
            Assert.Null(dto.PhoneNumber);
        }

        [Fact]
        public void UserCreateDto_Address_ShouldAcceptNull()
        {
            // Arrange
            var dto = new UserCreateDto();

            // Act
            dto.Address = null;

            // Assert
            Assert.Null(dto.Address);
        }

        [Fact]
        public void UserUpdateDto_PhoneNumber_ShouldAcceptNull()
        {
            // Arrange
            var dto = new UserUpdateDto();

            // Act
            dto.PhoneNumber = null;

            // Assert
            Assert.Null(dto.PhoneNumber);
        }

        [Fact]
        public void UserUpdateDto_Address_ShouldAcceptNull()
        {
            // Arrange
            var dto = new UserUpdateDto();

            // Act
            dto.Address = null;

            // Assert
            Assert.Null(dto.Address);
        }

        [Fact]
        public void UserDto_IsActive_ShouldToggle()
        {
            // Arrange
            var dto = new UserDto { IsActive = false };

            // Act
            dto.IsActive = true;

            // Assert
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void UserUpdateDto_IsActive_ShouldToggle()
        {
            // Arrange
            var dto = new UserUpdateDto { IsActive = false };

            // Act
            dto.IsActive = true;

            // Assert
            Assert.True(dto.IsActive);
        }
    }
}
