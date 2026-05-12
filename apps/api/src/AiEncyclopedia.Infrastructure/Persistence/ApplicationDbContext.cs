using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PdfDocument> PdfDocuments => Set<PdfDocument>();
    public DbSet<AIJob> AIJobs => Set<AIJob>();
    public DbSet<KnowledgeRelation> KnowledgeRelations => Set<KnowledgeRelation>();
    public DbSet<UserBookmark> UserBookmarks => Set<UserBookmark>();
    public DbSet<ReadingHistory> ReadingHistories => Set<ReadingHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is User u && entry.State == EntityState.Modified)
                u.UpdatedAt = DateTimeOffset.UtcNow;
            if (entry.Entity is Article a && entry.State == EntityState.Modified)
                a.UpdatedAt = DateTimeOffset.UtcNow;
            if (entry.Entity is Topic t && entry.State == EntityState.Modified)
                t.UpdatedAt = DateTimeOffset.UtcNow;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
