using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SoruHavuzu.Infrastructure.Persistence.Configurations;

public class SubTopicsConfiguration : IEntityTypeConfiguration<SubTopics>
{
    public void Configure(EntityTypeBuilder<SubTopics> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(s => s.Code)
            .HasMaxLength(32);

        // Doğal anahtar: aynı konu kapsamında aynı isimde iki kazanım olamaz.
        builder.HasIndex(s => new { s.TopicId, s.Name })
            .IsUnique();
    }
}
