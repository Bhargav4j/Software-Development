using Xunit;
using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Entities.Tests;

public class UserTests
{
    [Fact]
    public void User_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.NotNull(user);
        Assert.Equal(0, user.Id);
        Assert.Equal(string.Empty, user.Email);
        Assert.Equal(string.Empty, user.PasswordHash);
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
        Assert.Null(user.PhoneNumber);
        Assert.Null(user.Address);
        Assert.Equal("System", user.CreatedBy);
        Assert.Null(user.ModifiedBy);
        Assert.False(user.IsActive);
        Assert.NotNull(user.Bookings);
        Assert.Empty(user.Bookings);
    }

    [Fact]
    public void User_SetId_ShouldSetIdValue()
    {
        // Arrange
        var user = new User();
        var expectedId = 100;

        // Act
        user.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, user.Id);
    }

    [Fact]
    public void User_SetEmail_ShouldSetEmailValue()
    {
        // Arrange
        var user = new User();
        var expectedEmail = "test@example.com";

        // Act
        user.Email = expectedEmail;

        // Assert
        Assert.Equal(expectedEmail, user.Email);
    }

    [Theory]
    [InlineData("user@test.com")]
    [InlineData("admin@company.org")]
    [InlineData("test.user+tag@example.co.uk")]
    public void User_SetEmail_ShouldHandleDifferentEmailFormats(string email)
    {
        // Arrange
        var user = new User();

        // Act
        user.Email = email;

        // Assert
        Assert.Equal(email, user.Email);
    }

    [Fact]
    public void User_SetPasswordHash_ShouldSetPasswordHashValue()
    {
        // Arrange
        var user = new User();
        var expectedHash = "$2a$11$abcdefghijklmnopqrstuvwxyz123456";

        // Act
        user.PasswordHash = expectedHash;

        // Assert
        Assert.Equal(expectedHash, user.PasswordHash);
    }

    [Fact]
    public void User_SetFirstName_ShouldSetFirstNameValue()
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
    public void User_SetLastName_ShouldSetLastNameValue()
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
    public void User_SetPhoneNumber_ShouldSetPhoneNumberValue()
    {
        // Arrange
        var user = new User();
        var expectedPhone = "+1234567890";

        // Act
        user.PhoneNumber = expectedPhone;

        // Assert
        Assert.Equal(expectedPhone, user.PhoneNumber);
    }

    [Fact]
    public void User_SetPhoneNumber_ShouldAcceptNullValue()
    {
        // Arrange
        var user = new User { PhoneNumber = "1234567890" };

        // Act
        user.PhoneNumber = null;

        // Assert
        Assert.Null(user.PhoneNumber);
    }

    [Theory]
    [InlineData("+1-555-123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("5551234567")]
    public void User_SetPhoneNumber_ShouldHandleDifferentPhoneFormats(string phone)
    {
        // Arrange
        var user = new User();

        // Act
        user.PhoneNumber = phone;

        // Assert
        Assert.Equal(phone, user.PhoneNumber);
    }

    [Fact]
    public void User_SetAddress_ShouldSetAddressValue()
    {
        // Arrange
        var user = new User();
        var expectedAddress = "123 Main St, City, State 12345";

        // Act
        user.Address = expectedAddress;

        // Assert
        Assert.Equal(expectedAddress, user.Address);
    }

    [Fact]
    public void User_SetAddress_ShouldAcceptNullValue()
    {
        // Arrange
        var user = new User { Address = "123 Test St" };

        // Act
        user.Address = null;

        // Assert
        Assert.Null(user.Address);
    }

    [Fact]
    public void User_SetCreatedDate_ShouldSetCreatedDateValue()
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
    public void User_SetModifiedDate_ShouldSetModifiedDateValue()
    {
        // Arrange
        var user = new User();
        var expectedDate = new DateTime(2024, 6, 15);

        // Act
        user.ModifiedDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, user.ModifiedDate);
    }

    [Fact]
    public void User_SetModifiedDate_ShouldAcceptNullValue()
    {
        // Arrange
        var user = new User { ModifiedDate = DateTime.Now };

        // Act
        user.ModifiedDate = null;

        // Assert
        Assert.Null(user.ModifiedDate);
    }

    [Fact]
    public void User_SetIsActive_ShouldSetIsActiveValue()
    {
        // Arrange
        var user = new User();

        // Act
        user.IsActive = true;

        // Assert
        Assert.True(user.IsActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void User_SetIsActive_ShouldHandleBooleanValues(bool isActive)
    {
        // Arrange
        var user = new User();

        // Act
        user.IsActive = isActive;

        // Assert
        Assert.Equal(isActive, user.IsActive);
    }

    [Fact]
    public void User_SetCreatedBy_ShouldSetCreatedByValue()
    {
        // Arrange
        var user = new User();
        var expectedUser = "AdminUser";

        // Act
        user.CreatedBy = expectedUser;

        // Assert
        Assert.Equal(expectedUser, user.CreatedBy);
    }

    [Fact]
    public void User_SetModifiedBy_ShouldSetModifiedByValue()
    {
        // Arrange
        var user = new User();
        var expectedUser = "EditorUser";

        // Act
        user.ModifiedBy = expectedUser;

        // Assert
        Assert.Equal(expectedUser, user.ModifiedBy);
    }

    [Fact]
    public void User_SetModifiedBy_ShouldAcceptNullValue()
    {
        // Arrange
        var user = new User { ModifiedBy = "User1" };

        // Act
        user.ModifiedBy = null;

        // Assert
        Assert.Null(user.ModifiedBy);
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
    public void User_CompleteObject_ShouldSetAllProperties()
    {
        // Arrange
        var expectedId = 1;
        var expectedEmail = "john.doe@example.com";
        var expectedPasswordHash = "$2a$11$hashedpassword";
        var expectedFirstName = "John";
        var expectedLastName = "Doe";
        var expectedPhone = "+1234567890";
        var expectedAddress = "123 Main St";
        var expectedCreatedDate = new DateTime(2024, 1, 1);
        var expectedModifiedDate = new DateTime(2024, 6, 1);
        var expectedIsActive = true;
        var expectedCreatedBy = "Admin";
        var expectedModifiedBy = "Editor";

        // Act
        var user = new User
        {
            Id = expectedId,
            Email = expectedEmail,
            PasswordHash = expectedPasswordHash,
            FirstName = expectedFirstName,
            LastName = expectedLastName,
            PhoneNumber = expectedPhone,
            Address = expectedAddress,
            CreatedDate = expectedCreatedDate,
            ModifiedDate = expectedModifiedDate,
            IsActive = expectedIsActive,
            CreatedBy = expectedCreatedBy,
            ModifiedBy = expectedModifiedBy
        };

        // Assert
        Assert.Equal(expectedId, user.Id);
        Assert.Equal(expectedEmail, user.Email);
        Assert.Equal(expectedPasswordHash, user.PasswordHash);
        Assert.Equal(expectedFirstName, user.FirstName);
        Assert.Equal(expectedLastName, user.LastName);
        Assert.Equal(expectedPhone, user.PhoneNumber);
        Assert.Equal(expectedAddress, user.Address);
        Assert.Equal(expectedCreatedDate, user.CreatedDate);
        Assert.Equal(expectedModifiedDate, user.ModifiedDate);
        Assert.Equal(expectedIsActive, user.IsActive);
        Assert.Equal(expectedCreatedBy, user.CreatedBy);
        Assert.Equal(expectedModifiedBy, user.ModifiedBy);
    }

    [Fact]
    public void User_SetEmptyStrings_ShouldAcceptEmptyStrings()
    {
        // Arrange
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        user.Email = string.Empty;
        user.PasswordHash = string.Empty;
        user.FirstName = string.Empty;
        user.LastName = string.Empty;

        // Assert
        Assert.Equal(string.Empty, user.Email);
        Assert.Equal(string.Empty, user.PasswordHash);
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
    }

    [Fact]
    public void User_SetNegativeId_ShouldAcceptNegativeValue()
    {
        // Arrange
        var user = new User();
        var negativeId = -1;

        // Act
        user.Id = negativeId;

        // Assert
        Assert.Equal(negativeId, user.Id);
    }

    [Fact]
    public void User_SetInvalidEmail_ShouldAcceptAnyString()
    {
        // Arrange
        var user = new User();
        var invalidEmail = "notanemail";

        // Act
        user.Email = invalidEmail;

        // Assert
        Assert.Equal(invalidEmail, user.Email);
    }

    [Fact]
    public void User_FullName_ShouldCombineFirstAndLastNames()
    {
        // Arrange
        var user = new User
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = $"{user.FirstName} {user.LastName}";

        // Assert
        Assert.Equal("John Doe", fullName);
    }
}
