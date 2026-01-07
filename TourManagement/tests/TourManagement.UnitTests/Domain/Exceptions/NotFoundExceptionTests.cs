using Xunit;
using TourManagement.Domain.Exceptions;

namespace TourManagement.UnitTests.Domain.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void NotFoundException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void NotFoundException_WithEntityNameAndKey_ShouldFormatMessage()
    {
        // Arrange
        var entityName = "Tour";
        var key = 123;
        var expectedMessage = "Entity 'Tour' with key (123) was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_WithEntityNameAndStringKey_ShouldFormatMessage()
    {
        // Arrange
        var entityName = "User";
        var key = "test@example.com";
        var expectedMessage = "Entity 'User' with key (test@example.com) was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_ShouldBeOfTypeException()
    {
        // Arrange & Act
        var exception = new NotFoundException("Test message");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void NotFoundException_WithEmptyMessage_ShouldAccept()
    {
        // Arrange & Act
        var exception = new NotFoundException(string.Empty);

        // Assert
        Assert.Equal(string.Empty, exception.Message);
    }

    [Fact]
    public void NotFoundException_WithNullKey_ShouldHandleNull()
    {
        // Arrange
        var entityName = "Booking";
        object? key = null;
        var expectedMessage = "Entity 'Booking' with key () was not found.";

        // Act
        var exception = new NotFoundException(entityName, key!);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_CanBeCaught()
    {
        // Arrange
        var exceptionCaught = false;

        // Act
        try
        {
            throw new NotFoundException("Test", 1);
        }
        catch (NotFoundException)
        {
            exceptionCaught = true;
        }

        // Assert
        Assert.True(exceptionCaught);
    }
}
