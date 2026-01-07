using Xunit;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Infrastructure.Data.Configurations;

public class UserConfigurationTests
{
    private TourManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TourManagementDbContext(options);
    }

    [Fact]
    public void UserConfiguration_ShouldConfigureTableName()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal("UserInfo", entityType.GetTableName());
    }

    [Fact]
    public void UserConfiguration_ShouldConfigurePrimaryKey()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));
        var primaryKey = entityType!.FindPrimaryKey();

        // Assert
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void UserConfiguration_ShouldConfigureRequiredProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        // Act & Assert
        Assert.False(entityType!.FindProperty("Email")!.IsNullable);
        Assert.False(entityType.FindProperty("FirstName")!.IsNullable);
        Assert.False(entityType.FindProperty("LastName")!.IsNullable);
        Assert.False(entityType.FindProperty("Gender")!.IsNullable);
        Assert.False(entityType.FindProperty("PasswordHash")!.IsNullable);
        Assert.False(entityType.FindProperty("CreatedBy")!.IsNullable);
        Assert.False(entityType.FindProperty("IsActive")!.IsNullable);
    }

    [Fact]
    public void UserConfiguration_ShouldConfigureOptionalProperties()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        // Act & Assert
        Assert.True(entityType!.FindProperty("DateOfBirth")!.IsNullable);
        Assert.True(entityType.FindProperty("Street")!.IsNullable);
        Assert.True(entityType.FindProperty("City")!.IsNullable);
        Assert.True(entityType.FindProperty("State")!.IsNullable);
        Assert.True(entityType.FindProperty("ModifiedDate")!.IsNullable);
        Assert.True(entityType.FindProperty("ModifiedBy")!.IsNullable);
    }

    [Fact]
    public void UserConfiguration_ShouldConfigureMaxLengths()
    {
        // Arrange
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        // Act & Assert
        Assert.Equal(200, entityType!.FindProperty("Email")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("FirstName")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("LastName")!.GetMaxLength());
        Assert.Equal(20, entityType.FindProperty("Gender")!.GetMaxLength());
        Assert.Equal(500, entityType.FindProperty("PasswordHash")!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty("Street")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("City")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("State")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("CreatedBy")!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty("ModifiedBy")!.GetMaxLength());
    }

    [Fact]
    public async Task UserConfiguration_ShouldEnforceUniqueEmailConstraint()
    {
        // Arrange
        using var context = CreateContext();
        context.Users.Add(new User
        {
            Email = "duplicate@test.com",
            FirstName = "User1",
            LastName = "Test",
            Gender = "Male",
            PasswordHash = "hash",
            IsActive = true,
            CreatedBy = "Test"
        });
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            context.Users.Add(new User
            {
                Email = "duplicate@test.com",
                FirstName = "User2",
                LastName = "Test",
                Gender = "Female",
                PasswordHash = "hash",
                IsActive = true,
                CreatedBy = "Test"
            });
            await context.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task UserConfiguration_ShouldHaveBookingsRelationship()
    {
        // Arrange
        using var context = CreateContext();
        var user = new User
        {
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            Gender = "Male",
            PasswordHash = "hash",
            IsActive = true,
            CreatedBy = "Test"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var booking = new Booking
        {
            Email = user.Email,
            FirstName = user.FirstName,
            TourName = "Tour",
            Place = "Place",
            IsActive = true,
            CreatedBy = "Test"
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Act
        var loadedUser = await context.Users.Include(u => u.Bookings).FirstOrDefaultAsync(u => u.Id == user.Id);

        // Assert
        Assert.NotNull(loadedUser);
        Assert.NotNull(loadedUser.Bookings);
    }
}
