using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Booking
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("booking");

        builder.HasKey(b => b.BookingId);

        builder.Property(b => b.BookingId)
            .HasColumnName("TOUR_ID")
            .UseIdentityAlwaysColumn();

        builder.Property(b => b.TourId)
            .IsRequired();

        builder.Property(b => b.TourName)
            .HasMaxLength(50)
            .HasColumnName("TOUR_NAME");

        builder.Property(b => b.Place)
            .HasMaxLength(50)
            .HasColumnName("PLACE");

        builder.Property(b => b.Email)
            .HasMaxLength(50)
            .HasColumnName("Email");

        builder.Property(b => b.FirstName)
            .HasMaxLength(50)
            .HasColumnName("FirstName");

        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Pending");

        builder.Property(b => b.BookingDate)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(b => b.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(b => b.Tour)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TourId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.Email)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
