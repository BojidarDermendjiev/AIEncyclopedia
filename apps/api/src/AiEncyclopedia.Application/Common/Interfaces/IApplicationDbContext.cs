using AiEncyclopedia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Category> Categories { get; }
    DbSet<Topic> Topics { get; }
    DbSet<Article> Articles { get; }
    DbSet<Tag> Tags { get; }
    DbSet<ArticleTag> ArticleTags { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<PdfDocument> PdfDocuments { get; }
    DbSet<AIJob> AIJobs { get; }
    DbSet<KnowledgeRelation> KnowledgeRelations { get; }
    DbSet<UserBookmark> UserBookmarks { get; }
    DbSet<ReadingHistory> ReadingHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
