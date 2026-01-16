using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Tour
/// </summary>
public class TourConfiguration : IEntityTypeConfiguration<Tour>
{
    public void Configure(EntityTypeBuilder<Tour> builder)
    {
        builder.ToTable("Tour");

        builder.HasKey(t => t.TourId);

        builder.Property(t => t.TourId)
            .HasColumnName("TOUR_ID")
            .UseIdentityColumn(1, 1);

        builder.Property(t => t.TourName)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("TOUR_NAME");

        builder.Property(t => t.Place)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("PLACE");

        builder.Property(t => t.Days)
            .IsRequired()
            .HasColumnName("DAYS");

        builder.Property(t => t.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasColumnName("PRICE");

        builder.Property(t => t.Locations)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("LOCATIONS");

        builder.Property(t => t.TourInfo)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("TOUR_INFO");

        builder.Property(t => t.PicturePath)
            .HasMaxLength(200)
            .HasColumnName("pic");

        builder.Property(t => t.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(t => t.Bookings)
            .WithOne(b => b.Tour)
            .HasForeignKey(b => b.TourId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
