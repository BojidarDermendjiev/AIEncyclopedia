using AiEncyclopedia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiEncyclopedia.Infrastructure.Persistence.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(255).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(300).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Property(t => t.DifficultyLevel).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Topics)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
