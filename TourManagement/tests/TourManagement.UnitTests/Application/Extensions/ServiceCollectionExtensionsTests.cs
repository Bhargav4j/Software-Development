using Xunit;
using Microsoft.Extensions.DependencyInjection;
using TourManagement.Application.Extensions;
using TourManagement.Application.Services;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.UnitTests.Application.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterTourService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var tourService = serviceProvider.GetService<ITourService>();
        Assert.NotNull(tourService);
        Assert.IsType<TourService>(tourService);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterUserService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userService = serviceProvider.GetService<IUserService>();
        Assert.NotNull(userService);
        Assert.IsType<UserService>(userService);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterBookingService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var bookingService = serviceProvider.GetService<IBookingService>();
        Assert.NotNull(bookingService);
        Assert.IsType<BookingService>(bookingService);
    }

    [Fact]
    public void AddApplicationServices_ShouldRegisterServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();

        // Assert
        var tourServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITourService));
        Assert.NotNull(tourServiceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, tourServiceDescriptor.Lifetime);

        var userServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserService));
        Assert.NotNull(userServiceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, userServiceDescriptor.Lifetime);

        var bookingServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBookingService));
        Assert.NotNull(bookingServiceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, bookingServiceDescriptor.Lifetime);
    }

    [Fact]
    public void AddApplicationServices_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }
}
