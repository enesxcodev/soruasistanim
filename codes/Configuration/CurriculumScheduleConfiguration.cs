using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoruHavuzu.Domain.Entities;

namespace SoruHavuzu.Infrastructure.Persistence.Configurations;

public class CurriculumScheduleConfiguration : IEntityTypeConfiguration<CurriculumSchedule>
{
    public void Configure(EntityTypeBuilder<CurriculumSchedule> builder)
    {
        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.SchoolWeek)
            .IsRequired();

        // Relationships
        builder.HasOne(cs => cs.Grade)
            .WithMany(g => g.CurriculumSchedules)
            .HasForeignKey(cs => cs.GradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Lesson)
            .WithMany(l => l.CurriculumSchedules)
            .HasForeignKey(cs => cs.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Topic)
            .WithMany(t => t.CurriculumSchedules)
            .HasForeignKey(cs => cs.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
