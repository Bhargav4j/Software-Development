using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Repositories;

namespace TourManagement.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<TourManagementDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30));
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
            })
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUserInfoRepository, UserInfoRepository>();
        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
