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
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("booking_id")
            .ValueGeneratedOnAdd();

        builder.Property(b => b.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(b => b.TourId)
            .HasColumnName("tour_id")
            .IsRequired();

        builder.Property(b => b.BookingDate)
            .HasColumnName("booking_date")
            .IsRequired();

        builder.Property(b => b.NumberOfPeople)
            .HasColumnName("number_of_people")
            .IsRequired();

        builder.Property(b => b.TotalAmount)
            .HasColumnName("total_amount")
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(b => b.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

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
