using Xunit;
using TourManagement.Domain.Entities;

namespace Tests.TourManagement.Domain.Entities;

/// <summary>
/// Tests for Admin entity
/// </summary>
public class AdminTests
{
    [Fact]
    public void Admin_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var admin = new Admin();

        // Assert
        Assert.Equal(0, admin.Id);
        Assert.Equal(string.Empty, admin.Email);
        Assert.Equal(string.Empty, admin.Password);
        Assert.Equal(string.Empty, admin.Name);
        Assert.False(admin.IsActive);
        Assert.Equal("System", admin.CreatedBy);
        Assert.Null(admin.ModifiedBy);
    }

    [Fact]
    public void Admin_SetProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var admin = new Admin();
        var createdDate = DateTime.UtcNow;

        // Act
        admin.Id = 1;
        admin.Email = "admin@example.com";
        admin.Password = "hashedPassword123";
        admin.Name = "John Admin";
        admin.CreatedDate = createdDate;
        admin.IsActive = true;
        admin.CreatedBy = "SuperAdmin";

        // Assert
        Assert.Equal(1, admin.Id);
        Assert.Equal("admin@example.com", admin.Email);
        Assert.Equal("hashedPassword123", admin.Password);
        Assert.Equal("John Admin", admin.Name);
        Assert.True(admin.IsActive);
        Assert.Equal("SuperAdmin", admin.CreatedBy);
    }

    [Fact]
    public void Admin_Email_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var admin = new Admin { Email = "" };

        // Assert
        Assert.Equal(string.Empty, admin.Email);
    }

    [Fact]
    public void Admin_Email_ShouldAcceptValidEmail()
    {
        // Arrange & Act
        var admin = new Admin { Email = "admin@domain.com" };

        // Assert
        Assert.Equal("admin@domain.com", admin.Email);
    }

    [Fact]
    public void Admin_Password_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var admin = new Admin { Password = "" };

        // Assert
        Assert.Equal(string.Empty, admin.Password);
    }

    [Fact]
    public void Admin_Password_ShouldAcceptHashedPassword()
    {
        // Arrange & Act
        var hashedPassword = "$2a$11$abcdefghijklmnopqrstuv";
        var admin = new Admin { Password = hashedPassword };

        // Assert
        Assert.Equal(hashedPassword, admin.Password);
    }

    [Fact]
    public void Admin_Name_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var admin = new Admin { Name = "" };

        // Assert
        Assert.Equal(string.Empty, admin.Name);
    }

    [Fact]
    public void Admin_Name_ShouldAcceptFullName()
    {
        // Arrange & Act
        var admin = new Admin { Name = "Jane Doe" };

        // Assert
        Assert.Equal("Jane Doe", admin.Name);
    }

    [Fact]
    public void Admin_IsActive_ShouldToggleBetweenTrueAndFalse()
    {
        // Arrange
        var admin = new Admin { IsActive = true };

        // Act & Assert
        Assert.True(admin.IsActive);

        admin.IsActive = false;
        Assert.False(admin.IsActive);
    }

    [Fact]
    public void Admin_ModifiedDate_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var admin = new Admin();

        // Assert
        Assert.Null(admin.ModifiedDate);
    }

    [Fact]
    public void Admin_ModifiedDate_ShouldAcceptDateTime()
    {
        // Arrange
        var modifiedDate = DateTime.UtcNow;
        var admin = new Admin();

        // Act
        admin.ModifiedDate = modifiedDate;

        // Assert
        Assert.Equal(modifiedDate, admin.ModifiedDate);
    }

    [Fact]
    public void Admin_ModifiedBy_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var admin = new Admin();

        // Assert
        Assert.Null(admin.ModifiedBy);
    }

    [Fact]
    public void Admin_ModifiedBy_ShouldAcceptValue()
    {
        // Arrange & Act
        var admin = new Admin { ModifiedBy = "SuperAdmin" };

        // Assert
        Assert.Equal("SuperAdmin", admin.ModifiedBy);
    }

    [Fact]
    public void Admin_CreatedBy_ShouldDefaultToSystem()
    {
        // Arrange & Act
        var admin = new Admin();

        // Assert
        Assert.Equal("System", admin.CreatedBy);
    }

    [Fact]
    public void Admin_CreatedDate_ShouldAcceptDateTime()
    {
        // Arrange
        var createdDate = new DateTime(2024, 1, 1);
        var admin = new Admin();

        // Act
        admin.CreatedDate = createdDate;

        // Assert
        Assert.Equal(createdDate, admin.CreatedDate);
    }
}
