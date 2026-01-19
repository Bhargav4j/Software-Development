using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for UserInfo
/// </summary>
public class UserInfoConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.ToTable("UserInfo");

        builder.HasKey(u => u.Email);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Gender)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.DateOfBirth)
            .IsRequired()
            .HasColumnName("dob");

        builder.Property(u => u.Street)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.City)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.State)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.Email)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
