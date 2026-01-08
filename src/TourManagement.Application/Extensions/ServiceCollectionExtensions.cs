using Microsoft.Extensions.DependencyInjection;
using TourManagement.Application.Services;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITourService, TourService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
