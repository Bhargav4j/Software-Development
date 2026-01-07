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
        builder.ToTable("UserInfo");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Email)
            .HasColumnName("Email")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Gender)
            .HasColumnName("Gender")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("Password")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.DateOfBirth)
            .HasColumnName("dob");

        builder.Property(u => u.Street)
            .HasColumnName("Street")
            .HasMaxLength(200);

        builder.Property(u => u.City)
            .HasColumnName("City")
            .HasMaxLength(100);

        builder.Property(u => u.State)
            .HasColumnName("State")
            .HasMaxLength(100);

        builder.Property(u => u.CreatedDate)
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(u => u.ModifiedDate)
            .HasColumnName("ModifiedDate");

        builder.Property(u => u.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.ModifiedBy)
            .HasColumnName("ModifiedBy")
            .HasMaxLength(100);
    }
}
