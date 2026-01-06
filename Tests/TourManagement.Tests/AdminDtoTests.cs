using System;
using TourManagement.Application.DTOs;
using Xunit;

namespace TourManagement.Application.DTOs.Tests
{
    public class AdminDtoTests
    {
        [Fact]
        public void AdminDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new AdminDto();

            // Assert
            Assert.Equal(0, dto.Id);
            Assert.Equal(string.Empty, dto.Username);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FullName);
            Assert.False(dto.IsActive);
        }

        [Fact]
        public void AdminDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new AdminDto();

            // Act
            dto.Id = 1;
            dto.Username = "admin_user";
            dto.Email = "admin@example.com";
            dto.FullName = "Admin User";
            dto.IsActive = true;

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("admin_user", dto.Username);
            Assert.Equal("admin@example.com", dto.Email);
            Assert.Equal("Admin User", dto.FullName);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void AdminCreateDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new AdminCreateDto();

            // Assert
            Assert.Equal(string.Empty, dto.Username);
            Assert.Equal(string.Empty, dto.Password);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FullName);
        }

        [Fact]
        public void AdminCreateDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new AdminCreateDto();

            // Act
            dto.Username = "newadmin";
            dto.Password = "SecurePass123";
            dto.Email = "newadmin@example.com";
            dto.FullName = "New Admin";

            // Assert
            Assert.Equal("newadmin", dto.Username);
            Assert.Equal("SecurePass123", dto.Password);
            Assert.Equal("newadmin@example.com", dto.Email);
            Assert.Equal("New Admin", dto.FullName);
        }

        [Fact]
        public void AdminLoginDto_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var dto = new AdminLoginDto();

            // Assert
            Assert.Equal(string.Empty, dto.Username);
            Assert.Equal(string.Empty, dto.Password);
        }

        [Fact]
        public void AdminLoginDto_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var dto = new AdminLoginDto();

            // Act
            dto.Username = "adminlogin";
            dto.Password = "LoginPass123";

            // Assert
            Assert.Equal("adminlogin", dto.Username);
            Assert.Equal("LoginPass123", dto.Password);
        }

        [Fact]
        public void AdminDto_IsActive_ShouldToggle()
        {
            // Arrange
            var dto = new AdminDto { IsActive = false };

            // Act
            dto.IsActive = true;

            // Assert
            Assert.True(dto.IsActive);

            // Act
            dto.IsActive = false;

            // Assert
            Assert.False(dto.IsActive);
        }

        [Fact]
        public void AdminDto_Username_ShouldAcceptEmptyString()
        {
            // Arrange
            var dto = new AdminDto();

            // Act
            dto.Username = "";

            // Assert
            Assert.Equal("", dto.Username);
        }

        [Fact]
        public void AdminDto_Email_ShouldAcceptValidEmail()
        {
            // Arrange
            var dto = new AdminDto();

            // Act
            dto.Email = "valid.email@domain.com";

            // Assert
            Assert.Equal("valid.email@domain.com", dto.Email);
        }

        [Fact]
        public void AdminCreateDto_Password_ShouldStoreValue()
        {
            // Arrange
            var dto = new AdminCreateDto();

            // Act
            dto.Password = "ComplexPassword!123";

            // Assert
            Assert.Equal("ComplexPassword!123", dto.Password);
        }

        [Fact]
        public void AdminLoginDto_Password_ShouldStoreValue()
        {
            // Arrange
            var dto = new AdminLoginDto();

            // Act
            dto.Password = "MySecretPassword";

            // Assert
            Assert.Equal("MySecretPassword", dto.Password);
        }

        [Fact]
        public void AdminDto_FullName_ShouldAcceptLongNames()
        {
            // Arrange
            var dto = new AdminDto();
            var longName = "Alexander Benjamin Christopher Davidson";

            // Act
            dto.FullName = longName;

            // Assert
            Assert.Equal(longName, dto.FullName);
        }

        [Fact]
        public void AdminCreateDto_AllProperties_ShouldBeSettable()
        {
            // Arrange
            var dto = new AdminCreateDto();

            // Act
            dto.Username = "testuser";
            dto.Password = "testpass";
            dto.Email = "test@test.com";
            dto.FullName = "Test User";

            // Assert
            Assert.Equal("testuser", dto.Username);
            Assert.Equal("testpass", dto.Password);
            Assert.Equal("test@test.com", dto.Email);
            Assert.Equal("Test User", dto.FullName);
        }
    }
}
