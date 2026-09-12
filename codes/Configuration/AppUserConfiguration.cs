using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.PasswordResetTokenHash).HasMaxLength(500);

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Photo).HasMaxLength(500);

        // Relationships
        builder.HasOne(u => u.City)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.District)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.School)
            .WithMany(s => s.Users)
            .HasForeignKey(u => u.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Grade)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
