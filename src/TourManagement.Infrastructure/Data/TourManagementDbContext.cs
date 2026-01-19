using Microsoft.EntityFrameworkCore;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data;

/// <summary>
/// Database context for Tour Management application
/// </summary>
public class TourManagementDbContext : DbContext
{
    public TourManagementDbContext(DbContextOptions<TourManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserInfo> UserInfos { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for PostgreSQL
        modelBuilder.HasDefaultSchema("public");

        // Configure PostgreSQL extensions if needed
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TourManagementDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable legacy timestamp behavior for PostgreSQL compatibility
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}
