using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiEncyclopedia.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).HasMaxLength(500).IsRequired();
        builder.Property(a => a.Slug).HasMaxLength(550).IsRequired();
        builder.HasIndex(a => a.Slug).IsUnique();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.AiProvider).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.SeoTitle).HasMaxLength(70);
        builder.Property(a => a.SeoDescription).HasMaxLength(160);
        builder.Property(a => a.SeoKeywords).HasColumnType("text[]");

        builder.HasIndex(a => new { a.Status, a.PublishedAt });
        builder.HasIndex(a => a.TopicId);

        builder.HasOne(a => a.Topic)
            .WithMany(t => t.Articles)
            .HasForeignKey(a => a.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
