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
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(b => b.TourId)
            .HasColumnName("TOUR_ID");

        builder.Property(b => b.TourName)
            .HasColumnName("TOUR_NAME")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Place)
            .HasColumnName("PLACE")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Email)
            .HasColumnName("Email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.BookingDate)
            .HasColumnName("BookingDate")
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(b => b.CreatedDate)
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(b => b.ModifiedDate)
            .HasColumnName("ModifiedDate");

        builder.Property(b => b.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.ModifiedBy)
            .HasColumnName("ModifiedBy")
            .HasMaxLength(100);
    }
}
