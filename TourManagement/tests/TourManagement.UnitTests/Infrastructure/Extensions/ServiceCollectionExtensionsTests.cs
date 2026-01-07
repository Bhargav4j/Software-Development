using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Extensions;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Domain.Interfaces.Repositories;

namespace TourManagement.UnitTests.Infrastructure.Extensions;

public class ServiceCollectionExtensionsTests
{
    private IConfiguration CreateConfiguration(string connectionString = "Server=test;Database=test;User=test;Password=test;")
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:DefaultConnection", connectionString}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        services.AddLogging();

        // Act
        services.AddInfrastructureServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<TourManagementDbContext>();
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterTourRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        services.AddLogging();

        // Act
        services.AddInfrastructureServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var tourRepository = serviceProvider.GetService<ITourRepository>();
        Assert.NotNull(tourRepository);
        Assert.IsType<TourRepository>(tourRepository);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterUserRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        services.AddLogging();

        // Act
        services.AddInfrastructureServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var userRepository = serviceProvider.GetService<IUserRepository>();
        Assert.NotNull(userRepository);
        Assert.IsType<UserRepository>(userRepository);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterBookingRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        services.AddLogging();

        // Act
        services.AddInfrastructureServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var bookingRepository = serviceProvider.GetService<IBookingRepository>();
        Assert.NotNull(bookingRepository);
        Assert.IsType<BookingRepository>(bookingRepository);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterRepositoriesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        services.AddLogging();

        // Act
        services.AddInfrastructureServices(configuration);

        // Assert
        var tourRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITourRepository));
        Assert.NotNull(tourRepoDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, tourRepoDescriptor.Lifetime);

        var userRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserRepository));
        Assert.NotNull(userRepoDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, userRepoDescriptor.Lifetime);

        var bookingRepoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBookingRepository));
        Assert.NotNull(bookingRepoDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, bookingRepoDescriptor.Lifetime);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        services.AddLogging();

        // Act
        var result = services.AddInfrastructureServices(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddInfrastructureServices_ShouldThrowException_WhenConnectionStringNotFound()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddLogging();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.AddInfrastructureServices(configuration));
    }
}
