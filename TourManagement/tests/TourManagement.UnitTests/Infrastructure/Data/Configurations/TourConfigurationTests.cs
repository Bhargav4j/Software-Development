using Xunit;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Data.Configurations;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Infrastructure.Data.Configurations;

public class TourConfigurationTests
{
    private TourManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourManagementDbContext(options);
    }

    [Fact]
    public void TourConfiguration_ShouldConfigureTableName()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Tour));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal("Tour", entityType.GetTableName());
    }

    [Fact]
    public void TourConfiguration_ShouldConfigurePrimaryKey()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Tour));
        var primaryKey = entityType!.FindPrimaryKey();

        // Assert
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void TourConfiguration_ShouldConfigureRequiredProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Tour));

        // Act & Assert
        Assert.False(entityType!.FindProperty("TourName")!.IsNullable);
        Assert.False(entityType.FindProperty("Place")!.IsNullable);
        Assert.False(entityType.FindProperty("Days")!.IsNullable);
        Assert.False(entityType.FindProperty("Price")!.IsNullable);
        Assert.False(entityType.FindProperty("Locations")!.IsNullable);
        Assert.False(entityType.FindProperty("CreatedBy")!.IsNullable);
        Assert.False(entityType.FindProperty("IsActive")!.IsNullable);
    }

    [Fact]
    public void TourConfiguration_ShouldConfigureOptionalProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Tour));

        // Act & Assert
        Assert.True(entityType!.FindProperty("TourInfo")!.IsNullable);
        Assert.True(entityType.FindProperty("PicturePath")!.IsNullable);
        Assert.True(entityType.FindProperty("ModifiedDate")!.IsNullable);
        Assert.True(entityType.FindProperty("ModifiedBy")!.IsNullable);
    }

    [Fact]
    public void TourConfiguration_ShouldConfigureMaxLengths()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Tour));

        // Act & Assert
        Assert.Equal(200, entityType!.FindProperty("TourName")!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty("Place")!.GetMaxLength());
        Assert.Equal(500, entityType.FindProperty("Locations")!.GetMaxLength());
        Assert.Equal(2000, entityType.FindProperty("TourInfo")!.GetMaxLength());
        Assert.Equal(500, entityType.FindProperty("PicturePath")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("CreatedBy")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("ModifiedBy")!.GetMaxLength());
    }

    [Fact]
    public async Task TourConfiguration_ShouldHaveBookingsRelationship()
    {
        // Arrange
        using var context = CreateContext();
        var tour = new Tour { TourName = "Test", Place = "Place", IsActive = true, CreatedBy = "Test", Days = 1, Price = 100, Locations = "Loc" };
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        var booking = new Booking { TourId = tour.Id, TourName = "Test", Place = "Place", Email = "test@test.com", FirstName = "Test", IsActive = true, CreatedBy = "Test" };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Act
        var loadedTour = await context.Tours.Include(t => t.Bookings).FirstOrDefaultAsync(t => t.Id == tour.Id);

        // Assert
        Assert.NotNull(loadedTour);
        Assert.Single(loadedTour.Bookings);
    }
}
