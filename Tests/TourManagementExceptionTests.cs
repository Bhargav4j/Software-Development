using Xunit;
using TourManagement.Domain.Exceptions;

namespace Tests.TourManagement.Domain.Exceptions;

/// <summary>
/// Tests for TourManagementException
/// </summary>
public class TourManagementExceptionTests
{
    [Fact]
    public void TourManagementException_DefaultConstructor_ShouldCreateException()
    {
        // Arrange & Act
        var exception = new TourManagementException();

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<TourManagementException>(exception);
    }

    [Fact]
    public void TourManagementException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "An error occurred";

        // Act
        var exception = new TourManagementException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void TourManagementException_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "An error occurred";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new TourManagementException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void TourManagementException_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new TourManagementException();

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }
}

/// <summary>
/// Tests for EntityNotFoundException
/// </summary>
public class EntityNotFoundExceptionTests
{
    [Fact]
    public void EntityNotFoundException_Constructor_ShouldCreateExceptionWithFormattedMessage()
    {
        // Arrange
        var entityName = "User";
        var id = 123;

        // Act
        var exception = new EntityNotFoundException(entityName, id);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"{entityName} with ID {id} was not found.", exception.Message);
    }

    [Fact]
    public void EntityNotFoundException_ShouldInheritFromTourManagementException()
    {
        // Arrange & Act
        var exception = new EntityNotFoundException("Tour", 1);

        // Assert
        Assert.IsAssignableFrom<TourManagementException>(exception);
    }

    [Fact]
    public void EntityNotFoundException_WithDifferentEntityTypes_ShouldFormatCorrectly()
    {
        // Arrange & Act
        var userException = new EntityNotFoundException("User", 1);
        var tourException = new EntityNotFoundException("Tour", 99);
        var bookingException = new EntityNotFoundException("Booking", 456);

        // Assert
        Assert.Equal("User with ID 1 was not found.", userException.Message);
        Assert.Equal("Tour with ID 99 was not found.", tourException.Message);
        Assert.Equal("Booking with ID 456 was not found.", bookingException.Message);
    }

    [Fact]
    public void EntityNotFoundException_WithZeroId_ShouldFormatCorrectly()
    {
        // Arrange & Act
        var exception = new EntityNotFoundException("Admin", 0);

        // Assert
        Assert.Equal("Admin with ID 0 was not found.", exception.Message);
    }

    [Fact]
    public void EntityNotFoundException_WithNegativeId_ShouldFormatCorrectly()
    {
        // Arrange & Act
        var exception = new EntityNotFoundException("User", -1);

        // Assert
        Assert.Equal("User with ID -1 was not found.", exception.Message);
    }
}
