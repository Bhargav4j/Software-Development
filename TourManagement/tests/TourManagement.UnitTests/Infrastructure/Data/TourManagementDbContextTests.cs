using Xunit;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.UnitTests.Infrastructure.Data;

public class TourManagementDbContextTests
{
    private DbContextOptions<TourManagementDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<TourManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void TourManagementDbContext_Constructor_ShouldInitialize()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        using var context = new TourManagementDbContext(options);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Tours);
        Assert.NotNull(context.Users);
        Assert.NotNull(context.Bookings);
    }

    [Fact]
    public void TourManagementDbContext_Tours_ShouldBeEmpty()
    {
        // Arrange
        var options = CreateOptions();
        using var context = new TourManagementDbContext(options);

        // Act
        var tours = context.Tours.ToList();

        // Assert
        Assert.Empty(tours);
    }

    [Fact]
    public void TourManagementDbContext_Users_ShouldBeEmpty()
    {
        // Arrange
        var options = CreateOptions();
        using var context = new TourManagementDbContext(options);

        // Act
        var users = context.Users.ToList();

        // Assert
        Assert.Empty(users);
    }

    [Fact]
    public void TourManagementDbContext_Bookings_ShouldBeEmpty()
    {
        // Arrange
        var options = CreateOptions();
        using var context = new TourManagementDbContext(options);

        // Act
        var bookings = context.Bookings.ToList();

        // Assert
        Assert.Empty(bookings);
    }

    [Fact]
    public async Task TourManagementDbContext_CanAddTour()
    {
        // Arrange
        var options = CreateOptions();
        using var context = new TourManagementDbContext(options);
        var tour = new Tour
        {
            TourName = "Test Tour",
            Place = "Test Place",
            Days = 5,
            Price = 500m,
            Locations = "Location1, Location2",
            CreatedBy = "Test",
            IsActive = true
        };

        // Act
        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        // Assert
        var savedTour = await context.Tours.FirstOrDefaultAsync();
        Assert.NotNull(savedTour);
        Assert.Equal("Test Tour", savedTour.TourName);
    }

    [Fact]
    public async Task TourManagementDbContext_CanAddUser()
    {
        // Arrange
        var options = CreateOptions();
        using var context = new TourManagementDbContext(options);
        var user = new User
        {
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            Gender = "Male",
            PasswordHash = "hashedpassword",
            CreatedBy = "Test",
            IsActive = true
        };

        // Act
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Assert
        var savedUser = await context.Users.FirstOrDefaultAsync();
        Assert.NotNull(savedUser);
        Assert.Equal("test@test.com", savedUser.Email);
    }

    [Fact]
    public async Task TourManagementDbContext_CanAddBooking()
    {
        // Arrange
        var options = CreateOptions();
        using var context = new TourManagementDbContext(options);
        var booking = new Booking
        {
            TourName = "Test Tour",
            Place = "Test Place",
            Email = "test@test.com",
            FirstName = "John",
            CreatedBy = "Test",
            IsActive = true
        };

        // Act
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // Assert
        var savedBooking = await context.Bookings.FirstOrDefaultAsync();
        Assert.NotNull(savedBooking);
        Assert.Equal("test@test.com", savedBooking.Email);
    }

    [Fact]
    public async Task TourManagementDbContext_CanQueryTours()
    {
        // Arrange
        var options = CreateOptions();
        using (var context = new TourManagementDbContext(options))
        {
            context.Tours.AddRange(
                new Tour { TourName = "Tour 1", Place = "Place 1", CreatedBy = "Test", IsActive = true, Days = 1, Price = 100, Locations = "Loc1" },
                new Tour { TourName = "Tour 2", Place = "Place 2", CreatedBy = "Test", IsActive = true, Days = 2, Price = 200, Locations = "Loc2" }
            );
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new TourManagementDbContext(options))
        {
            var tours = await context.Tours.ToListAsync();

            // Assert
            Assert.Equal(2, tours.Count);
        }
    }
}
