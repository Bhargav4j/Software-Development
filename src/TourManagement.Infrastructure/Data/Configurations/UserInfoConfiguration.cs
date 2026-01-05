using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for UserInfo entity
/// </summary>
public class UserInfoConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.ToTable("UserInfo");

        builder.HasKey(u => u.Email);

        builder.Property(u => u.Email)
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.FirstName)
            .HasColumnName("FirstName")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .HasColumnName("LastName")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Gender)
            .HasColumnName("Gender")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(u => u.Password)
            .HasColumnName("Password")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.DateOfBirth)
            .HasColumnName("dob")
            .IsRequired();

        builder.Property(u => u.Street)
            .HasColumnName("Street")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.City)
            .HasColumnName("City")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.State)
            .HasColumnName("State")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.Email)
            .HasPrincipalKey(u => u.Email)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("CK_Gender", "[Gender]='Female' OR [Gender]='Male'"));
    }
}
