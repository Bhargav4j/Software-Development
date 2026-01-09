using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data;

/// <summary>
/// Database context for Tour Management application
/// </summary>
public class TourManagementDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public TourManagementDbContext(DbContextOptions<TourManagementDbContext> options, IConfiguration? configuration = null)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Tour> Tours { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var schema = _configuration?.GetValue<string>("Database:Schema") ?? Environment.GetEnvironmentVariable("DB_SCHEMA") ?? "public";
        modelBuilder.HasDefaultSchema(schema);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TourManagementDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}
