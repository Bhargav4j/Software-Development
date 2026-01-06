using System;
using TourManagement.Domain.Entities;
using Xunit;

namespace TourManagement.Domain.Entities.Tests
{
    public class AdminTests
    {
        [Fact]
        public void Admin_Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var admin = new Admin();

            // Assert
            Assert.Equal(0, admin.Id);
            Assert.Equal(string.Empty, admin.Username);
            Assert.Equal(string.Empty, admin.PasswordHash);
            Assert.Equal(string.Empty, admin.Email);
            Assert.Equal(string.Empty, admin.FullName);
            Assert.Equal(default(DateTime), admin.CreatedDate);
            Assert.Null(admin.ModifiedDate);
            Assert.False(admin.IsActive);
            Assert.Equal(string.Empty, admin.CreatedBy);
            Assert.Null(admin.ModifiedBy);
        }

        [Fact]
        public void Admin_SetProperties_ShouldUpdateValues()
        {
            // Arrange
            var admin = new Admin();
            var createdDate = DateTime.Now;
            var modifiedDate = DateTime.Now.AddDays(1);

            // Act
            admin.Id = 1;
            admin.Username = "admin_user";
            admin.PasswordHash = "hashed_password";
            admin.Email = "admin@example.com";
            admin.FullName = "Admin User";
            admin.CreatedDate = createdDate;
            admin.ModifiedDate = modifiedDate;
            admin.IsActive = true;
            admin.CreatedBy = "system";
            admin.ModifiedBy = "superadmin";

            // Assert
            Assert.Equal(1, admin.Id);
            Assert.Equal("admin_user", admin.Username);
            Assert.Equal("hashed_password", admin.PasswordHash);
            Assert.Equal("admin@example.com", admin.Email);
            Assert.Equal("Admin User", admin.FullName);
            Assert.Equal(createdDate, admin.CreatedDate);
            Assert.Equal(modifiedDate, admin.ModifiedDate);
            Assert.True(admin.IsActive);
            Assert.Equal("system", admin.CreatedBy);
            Assert.Equal("superadmin", admin.ModifiedBy);
        }

        [Fact]
        public void Admin_Username_ShouldAcceptValidUsername()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.Username = "admin123";

            // Assert
            Assert.Equal("admin123", admin.Username);
        }

        [Fact]
        public void Admin_Username_ShouldAcceptEmptyString()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.Username = "";

            // Assert
            Assert.Equal("", admin.Username);
        }

        [Fact]
        public void Admin_Email_ShouldAcceptValidEmail()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.Email = "valid.email@domain.com";

            // Assert
            Assert.Equal("valid.email@domain.com", admin.Email);
        }

        [Fact]
        public void Admin_Email_ShouldAcceptEmptyString()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.Email = "";

            // Assert
            Assert.Equal("", admin.Email);
        }

        [Fact]
        public void Admin_PasswordHash_ShouldStoreHashedValue()
        {
            // Arrange
            var admin = new Admin();
            var hashedPassword = "SHA256_HASHED_VALUE";

            // Act
            admin.PasswordHash = hashedPassword;

            // Assert
            Assert.Equal(hashedPassword, admin.PasswordHash);
        }

        [Fact]
        public void Admin_FullName_ShouldAcceptLongNames()
        {
            // Arrange
            var admin = new Admin();
            var longName = "Alexander Benjamin Christopher Davidson";

            // Act
            admin.FullName = longName;

            // Assert
            Assert.Equal(longName, admin.FullName);
        }

        [Fact]
        public void Admin_ModifiedBy_ShouldAcceptNull()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.ModifiedBy = null;

            // Assert
            Assert.Null(admin.ModifiedBy);
        }

        [Fact]
        public void Admin_ModifiedDate_ShouldAcceptNull()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.ModifiedDate = null;

            // Assert
            Assert.Null(admin.ModifiedDate);
        }

        [Fact]
        public void Admin_IsActive_ShouldToggle()
        {
            // Arrange
            var admin = new Admin { IsActive = false };

            // Act
            admin.IsActive = true;

            // Assert
            Assert.True(admin.IsActive);

            // Act
            admin.IsActive = false;

            // Assert
            Assert.False(admin.IsActive);
        }

        [Fact]
        public void Admin_AllStringProperties_ShouldHandleEmptyStrings()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.Username = "";
            admin.PasswordHash = "";
            admin.Email = "";
            admin.FullName = "";
            admin.CreatedBy = "";

            // Assert
            Assert.Equal("", admin.Username);
            Assert.Equal("", admin.PasswordHash);
            Assert.Equal("", admin.Email);
            Assert.Equal("", admin.FullName);
            Assert.Equal("", admin.CreatedBy);
        }

        [Fact]
        public void Admin_CreatedDate_ShouldStoreDateTime()
        {
            // Arrange
            var admin = new Admin();
            var date = new DateTime(2023, 1, 15, 10, 30, 0);

            // Act
            admin.CreatedDate = date;

            // Assert
            Assert.Equal(date, admin.CreatedDate);
        }

        [Fact]
        public void Admin_Id_ShouldAcceptPositiveValue()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.Id = 100;

            // Assert
            Assert.Equal(100, admin.Id);
        }

        [Fact]
        public void Admin_Id_ShouldAcceptNegativeValue()
        {
            // Arrange
            var admin = new Admin();

            // Act
            admin.Id = -1;

            // Assert
            Assert.Equal(-1, admin.Id);
        }
    }
}
