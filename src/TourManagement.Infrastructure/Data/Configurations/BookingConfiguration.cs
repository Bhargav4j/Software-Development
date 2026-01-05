using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Booking entity
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("booking");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("TOUR_ID")
            .ValueGeneratedOnAdd();

        builder.Property(b => b.TourName)
            .HasColumnName("TOUR_NAME")
            .HasMaxLength(50);

        builder.Property(b => b.Place)
            .HasColumnName("PLACE")
            .HasMaxLength(50);

        builder.Property(b => b.Email)
            .HasColumnName("Email")
            .HasMaxLength(50);

        builder.Property(b => b.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(50);

        builder.Property(b => b.BookingDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(b => b.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.ModifiedBy)
            .HasMaxLength(100);

        builder.HasOne(b => b.Tour)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TourId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.Email)
            .HasPrincipalKey(u => u.Email)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
