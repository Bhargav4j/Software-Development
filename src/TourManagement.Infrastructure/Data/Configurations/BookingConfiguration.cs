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
        builder.ToTable("Booking");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("BOOKING_ID");

        builder.Property(b => b.UserId)
            .HasColumnName("USER_ID");

        builder.Property(b => b.TourId)
            .HasColumnName("TOUR_ID");

        builder.Property(b => b.BookingDate)
            .HasColumnName("BOOKING_DATE");

        builder.Property(b => b.NumberOfPeople)
            .HasColumnName("NUMBER_OF_PEOPLE");

        builder.Property(b => b.TotalAmount)
            .HasColumnName("TOTAL_AMOUNT")
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(b => b.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Tour)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TourId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
