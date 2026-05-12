using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Domain.Entities;

public class Article
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public int ReadingTimeMinutes { get; set; }
    public long ViewCount { get; set; }
    public bool AiGenerated { get; set; }
    public AIProviderName? AiProvider { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string[]? SeoKeywords { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Topic Topic { get; set; } = null!;
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    public ICollection<PdfDocument> PdfDocuments { get; set; } = new List<PdfDocument>();
}
