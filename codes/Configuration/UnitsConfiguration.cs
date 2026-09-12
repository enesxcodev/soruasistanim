using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Infrastructure.Persistence.Configurations;

public class UnitsConfiguration : IEntityTypeConfiguration<Units>
{
    public void Configure(EntityTypeBuilder<Units> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Code)
            .HasMaxLength(32);

        // Doğal anahtar: aynı sınıf+ders kapsamında aynı isimde iki ünite/tema olamaz.
        builder.HasIndex(u => new { u.GradeId, u.LessonId, u.Name })
            .IsUnique();

        // Relationships
        builder.HasOne(u => u.Lesson)
            .WithMany(l => l.Units)
            .HasForeignKey(u => u.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Grade)
            .WithMany(g => g.Units)
            .HasForeignKey(u => u.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
