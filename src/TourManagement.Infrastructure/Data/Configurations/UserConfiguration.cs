using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Userinfo");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("user_id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FirstName)
            .HasColumnName("fname")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasColumnName("lname")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasColumnName("phone")
            .HasMaxLength(50);

        builder.Property(u => u.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(u => u.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.IsAdmin)
            .HasDefaultValue(false);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
