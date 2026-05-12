using AiEncyclopedia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiEncyclopedia.Infrastructure.Persistence.Configurations;

public class UserBookmarkConfiguration : IEntityTypeConfiguration<UserBookmark>
{
    public void Configure(EntityTypeBuilder<UserBookmark> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => new { b.UserId, b.ArticleId }).IsUnique();
        builder.HasOne(b => b.User).WithMany(u => u.Bookmarks).HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(b => b.Article).WithMany().HasForeignKey(b => b.ArticleId).OnDelete(DeleteBehavior.Cascade);
    }
}
