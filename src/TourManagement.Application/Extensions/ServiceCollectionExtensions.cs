using Microsoft.Extensions.DependencyInjection;
using TourManagement.Application.Mappings;
using TourManagement.Application.Services;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Extensions;

/// <summary>
/// Extension methods for service registration in the Application layer
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

        services.AddScoped<ITourService, TourServiceImpl>();
        services.AddScoped<IUserService, UserServiceImpl>();
        services.AddScoped<IBookingService, BookingServiceImpl>();
        services.AddScoped<TourFacade>();
        services.AddScoped<UserFacade>();
        services.AddScoped<BookingFacade>();

        return services;
    }
}
