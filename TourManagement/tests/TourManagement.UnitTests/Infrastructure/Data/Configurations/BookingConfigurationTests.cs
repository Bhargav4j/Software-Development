using Xunit;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Infrastructure.Data.Configurations;

public class BookingConfigurationTests
{
    private TourManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourManagementDbContext(options);
    }

    [Fact]
    public void BookingConfiguration_ShouldConfigureTableName()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Booking));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal("booking", entityType.GetTableName());
    }

    [Fact]
    public void BookingConfiguration_ShouldConfigurePrimaryKey()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Booking));
        var primaryKey = entityType!.FindPrimaryKey();

        // Assert
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void BookingConfiguration_ShouldConfigureRequiredProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Booking));

        // Act & Assert
        Assert.False(entityType!.FindProperty("TourName")!.IsNullable);
        Assert.False(entityType.FindProperty("Place")!.IsNullable);
        Assert.False(entityType.FindProperty("Email")!.IsNullable);
        Assert.False(entityType.FindProperty("FirstName")!.IsNullable);
        Assert.False(entityType.FindProperty("BookingDate")!.IsNullable);
        Assert.False(entityType.FindProperty("CreatedBy")!.IsNullable);
        Assert.False(entityType.FindProperty("IsActive")!.IsNullable);
    }

    [Fact]
    public void BookingConfiguration_ShouldConfigureOptionalProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Booking));

        // Act & Assert
        Assert.True(entityType!.FindProperty("TourId")!.IsNullable);
        Assert.True(entityType.FindProperty("ModifiedDate")!.IsNullable);
        Assert.True(entityType.FindProperty("ModifiedBy")!.IsNullable);
    }

    [Fact]
    public void BookingConfiguration_ShouldConfigureMaxLengths()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Booking));

        // Act & Assert
        Assert.Equal(200, entityType!.FindProperty("TourName")!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty("Place")!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty("Email")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("FirstName")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("CreatedBy")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("ModifiedBy")!.GetMaxLength());
    }

    [Fact]
    public async Task BookingConfiguration_ShouldConfigureForeignKeyToTour()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Test Tour", Place = "Place", IsActive = true, CreatedBy = "Test", Days = 5, Price = 500, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        var booking = new Booking
        {
            TourId = tour.Id,
            TourName = "Test Tour",
            Place = "Place",
            Email = "test@test.com",
            FirstName = "Test",
            IsActive = true,
            CreatedBy = "Test"
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Act
        var loadedBooking = await context.Bookings.Include(b => b.Tour).FirstOrDefaultAsync(b => b.Id == booking.Id);

        // Assert
        Assert.NotNull(loadedBooking);
        Assert.NotNull(loadedBooking.Tour);
        Assert.Equal(tour.Id, loadedBooking.Tour.Id);
    }

    [Fact]
    public async Task BookingConfiguration_ShouldAllowNullTourId()
    {
        // Arrange
        using var context = CreateContext();
        var booking = new Booking
        {
            TourId = null,
            TourName = "Test Tour",
            Place = "Place",
            Email = "test@test.com",
            FirstName = "Test",
            IsActive = true,
            CreatedBy = "Test"
        };

        // Act
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Assert
        var savedBooking = await context.Bookings.FirstOrDefaultAsync();
        Assert.NotNull(savedBooking);
        Assert.Null(savedBooking.TourId);
    }
}
