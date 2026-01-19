using Xunit;
using TourManagement.Domain.Exceptions;

namespace TourManagement.Domain.Exceptions.Tests;

public class NotFoundExceptionTests
{
    [Fact]
    public void NotFoundException_Constructor_WithMessage_ShouldCreateException()
    {
        // Arrange
        var expectedMessage = "Entity not found";

        // Act
        var exception = new NotFoundException(expectedMessage);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(expectedMessage, exception.Message);
        Assert.IsType<NotFoundException>(exception);
    }

    [Fact]
    public void NotFoundException_Constructor_WithMessage_ShouldInheritFromException()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Theory]
    [InlineData("User not found")]
    [InlineData("Tour not found")]
    [InlineData("Booking not found")]
    public void NotFoundException_Constructor_WithMessage_ShouldHandleDifferentMessages(string message)
    {
        // Arrange & Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void NotFoundException_Constructor_WithEntityAndKey_ShouldCreateException()
    {
        // Arrange
        var entityName = "User";
        var key = 123;
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("Tour", 1)]
    [InlineData("User", 100)]
    [InlineData("Booking", 999)]
    public void NotFoundException_Constructor_WithEntityAndKey_ShouldHandleDifferentEntitiesAndKeys(string entityName, int key)
    {
        // Arrange
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_Constructor_WithEntityAndKey_ShouldHandleStringKey()
    {
        // Arrange
        var entityName = "User";
        var key = "abc123";
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_Constructor_WithEntityAndKey_ShouldHandleGuidKey()
    {
        // Arrange
        var entityName = "Order";
        var key = Guid.NewGuid();
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_Constructor_WithEmptyMessage_ShouldAcceptEmptyString()
    {
        // Arrange
        var message = string.Empty;

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void NotFoundException_Constructor_WithNullKey_ShouldHandleNullKey()
    {
        // Arrange
        var entityName = "User";
        object? key = null;
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key!);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_Constructor_WithEmptyEntityName_ShouldAcceptEmptyString()
    {
        // Arrange
        var entityName = string.Empty;
        var key = 1;
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_Throw_WithMessage_ShouldThrowException()
    {
        // Arrange
        var message = "Entity not found";

        // Act & Assert
        void ThrowAction() => throw new NotFoundException(message);
        var exception = Assert.Throws<NotFoundException>(ThrowAction);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void NotFoundException_Throw_WithEntityAndKey_ShouldThrowException()
    {
        // Arrange
        var entityName = "User";
        var key = 123;
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act & Assert
        void ThrowAction() => throw new NotFoundException(entityName, key);
        var exception = Assert.Throws<NotFoundException>(ThrowAction);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_CatchAsException_ShouldCatchAsBaseException()
    {
        // Arrange
        var message = "Test exception";
        Exception? caughtException = null;

        // Act
        try
        {
            throw new NotFoundException(message);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<NotFoundException>(caughtException);
        Assert.Equal(message, caughtException.Message);
    }

    [Fact]
    public void NotFoundException_CatchAsNotFoundException_ShouldCatchSpecificException()
    {
        // Arrange
        var entityName = "Tour";
        var key = 999;
        NotFoundException? caughtException = null;

        // Act
        try
        {
            throw new NotFoundException(entityName, key);
        }
        catch (NotFoundException ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.Contains(entityName, caughtException.Message);
        Assert.Contains(key.ToString(), caughtException.Message);
    }

    [Theory]
    [InlineData("Tour", 0)]
    [InlineData("User", -1)]
    [InlineData("Booking", int.MaxValue)]
    [InlineData("Booking", int.MinValue)]
    public void NotFoundException_Constructor_WithEntityAndKey_ShouldHandleEdgeCaseKeys(string entityName, int key)
    {
        // Arrange
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void NotFoundException_MessageFormat_ShouldMatchExpectedFormat()
    {
        // Arrange
        var entityName = "Product";
        var key = 42;

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Contains("Product", exception.Message);
        Assert.Contains("42", exception.Message);
        Assert.Contains("was not found", exception.Message);
    }

    [Fact]
    public void NotFoundException_WithComplexKey_ShouldFormatKeyInMessage()
    {
        // Arrange
        var entityName = "Order";
        var key = new { Id = 1, Code = "ABC" };
        var expectedMessage = $"{entityName} with id '{key}' was not found.";

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Contains(entityName, exception.Message);
    }

    [Fact]
    public void NotFoundException_Constructor_WithLongMessage_ShouldHandleLongMessage()
    {
        // Arrange
        var longMessage = new string('a', 1000);

        // Act
        var exception = new NotFoundException(longMessage);

        // Assert
        Assert.Equal(longMessage, exception.Message);
        Assert.Equal(1000, exception.Message.Length);
    }
}
