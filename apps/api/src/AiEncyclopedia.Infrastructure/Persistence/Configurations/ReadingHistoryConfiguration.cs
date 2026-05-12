using AiEncyclopedia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiEncyclopedia.Infrastructure.Persistence.Configurations;

public class ReadingHistoryConfiguration : IEntityTypeConfiguration<ReadingHistory>
{
    public void Configure(EntityTypeBuilder<ReadingHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.HasIndex(h => new { h.UserId, h.ArticleId }).IsUnique();
        builder.HasOne(h => h.User).WithMany(u => u.ReadingHistory).HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(h => h.Article).WithMany().HasForeignKey(h => h.ArticleId).OnDelete(DeleteBehavior.Cascade);
    }
}
