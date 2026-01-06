using Xunit;
using TourManagement.Domain.Entities;

namespace Tests.TourManagement.Domain.Entities;

/// <summary>
/// Tests for User entity
/// </summary>
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
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
        Assert.Equal(string.Empty, user.Password);
        Assert.Null(user.Gender);
        Assert.Null(user.DateOfBirth);
        Assert.Null(user.Street);
        Assert.Null(user.City);
        Assert.Null(user.State);
        Assert.False(user.IsActive);
        Assert.Equal("System", user.CreatedBy);
        Assert.Null(user.ModifiedBy);
        Assert.NotNull(user.Bookings);
    }

    [Fact]
    public void User_SetProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var user = new User();
        var testDate = new DateTime(1990, 5, 15);

        // Act
        user.Id = 1;
        user.Email = "test@example.com";
        user.FirstName = "John";
        user.LastName = "Doe";
        user.Gender = "Male";
        user.Password = "hashedPassword123";
        user.DateOfBirth = testDate;
        user.Street = "123 Main St";
        user.City = "New York";
        user.State = "NY";
        user.IsActive = true;
        user.CreatedDate = DateTime.UtcNow;
        user.CreatedBy = "Admin";

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("Male", user.Gender);
        Assert.Equal("hashedPassword123", user.Password);
        Assert.Equal(testDate, user.DateOfBirth);
        Assert.Equal("123 Main St", user.Street);
        Assert.Equal("New York", user.City);
        Assert.Equal("NY", user.State);
        Assert.True(user.IsActive);
        Assert.Equal("Admin", user.CreatedBy);
    }

    [Fact]
    public void User_Email_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var user = new User { Email = "" };

        // Assert
        Assert.Equal(string.Empty, user.Email);
    }

    [Fact]
    public void User_Email_ShouldAcceptValidEmail()
    {
        // Arrange & Act
        var user = new User { Email = "user@domain.com" };

        // Assert
        Assert.Equal("user@domain.com", user.Email);
    }

    [Fact]
    public void User_OptionalFields_ShouldAcceptNullValues()
    {
        // Arrange & Act
        var user = new User
        {
            Gender = null,
            DateOfBirth = null,
            Street = null,
            City = null,
            State = null,
            ModifiedBy = null,
            ModifiedDate = null
        };

        // Assert
        Assert.Null(user.Gender);
        Assert.Null(user.DateOfBirth);
        Assert.Null(user.Street);
        Assert.Null(user.City);
        Assert.Null(user.State);
        Assert.Null(user.ModifiedBy);
        Assert.Null(user.ModifiedDate);
    }

    [Fact]
    public void User_Bookings_ShouldInitializeAsEmptyCollection()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.NotNull(user.Bookings);
        Assert.Empty(user.Bookings);
    }

    [Fact]
    public void User_Bookings_ShouldAllowAddingBookings()
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
    public void User_IsActive_ShouldToggleBetweenTrueAndFalse()
    {
        // Arrange
        var user = new User { IsActive = true };

        // Act & Assert
        Assert.True(user.IsActive);

        user.IsActive = false;
        Assert.False(user.IsActive);
    }

    [Fact]
    public void User_DateOfBirth_ShouldAcceptPastDate()
    {
        // Arrange
        var pastDate = new DateTime(1985, 3, 20);
        var user = new User();

        // Act
        user.DateOfBirth = pastDate;

        // Assert
        Assert.Equal(pastDate, user.DateOfBirth);
    }

    [Fact]
    public void User_ModifiedDate_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.Null(user.ModifiedDate);
    }
}
