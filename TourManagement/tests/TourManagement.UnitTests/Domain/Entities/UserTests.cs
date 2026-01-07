using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Domain.Entities;

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
        Assert.Equal(string.Empty, user.Gender);
        Assert.Equal(string.Empty, user.PasswordHash);
        Assert.Null(user.DateOfBirth);
        Assert.Null(user.Street);
        Assert.Null(user.City);
        Assert.Null(user.State);
        Assert.Equal(default(DateTime), user.CreatedDate);
        Assert.Null(user.ModifiedDate);
        Assert.False(user.IsActive);
        Assert.Equal(string.Empty, user.CreatedBy);
        Assert.Null(user.ModifiedBy);
        Assert.NotNull(user.Bookings);
        Assert.Empty(user.Bookings);
    }

    [Fact]
    public void User_Properties_ShouldBeSetAndGet()
    {
        // Arrange
        var user = new User();
        var testDate = DateTime.UtcNow;
        var birthDate = new DateTime(1990, 1, 1);

        // Act
        user.Id = 1;
        user.Email = "test@example.com";
        user.FirstName = "John";
        user.LastName = "Doe";
        user.Gender = "Male";
        user.PasswordHash = "hashed_password";
        user.DateOfBirth = birthDate;
        user.Street = "123 Main St";
        user.City = "New York";
        user.State = "NY";
        user.CreatedDate = testDate;
        user.ModifiedDate = testDate;
        user.IsActive = true;
        user.CreatedBy = "System";
        user.ModifiedBy = "Admin";

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("Male", user.Gender);
        Assert.Equal("hashed_password", user.PasswordHash);
        Assert.Equal(birthDate, user.DateOfBirth);
        Assert.Equal("123 Main St", user.Street);
        Assert.Equal("New York", user.City);
        Assert.Equal("NY", user.State);
        Assert.Equal(testDate, user.CreatedDate);
        Assert.Equal(testDate, user.ModifiedDate);
        Assert.True(user.IsActive);
        Assert.Equal("System", user.CreatedBy);
        Assert.Equal("Admin", user.ModifiedBy);
    }

    [Fact]
    public void User_Email_ShouldAcceptValidFormat()
    {
        // Arrange
        var user = new User();

        // Act
        user.Email = "user@domain.com";

        // Assert
        Assert.Equal("user@domain.com", user.Email);
    }

    [Fact]
    public void User_Bookings_ShouldAcceptNewItems()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        var booking = new Booking { Id = 1, Email = "test@example.com" };

        // Act
        user.Bookings.Add(booking);

        // Assert
        Assert.Single(user.Bookings);
        Assert.Contains(booking, user.Bookings);
    }

    [Fact]
    public void User_OptionalFields_CanBeNull()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Gender = "Male",
            PasswordHash = "hash",
            CreatedBy = "System"
        };

        // Assert
        Assert.Null(user.DateOfBirth);
        Assert.Null(user.Street);
        Assert.Null(user.City);
        Assert.Null(user.State);
        Assert.Null(user.ModifiedDate);
        Assert.Null(user.ModifiedBy);
    }

    [Fact]
    public void User_FullName_CanBeConcatenated()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var fullName = $"{user.FirstName} {user.LastName}";

        // Assert
        Assert.Equal("Jane Smith", fullName);
    }
}
