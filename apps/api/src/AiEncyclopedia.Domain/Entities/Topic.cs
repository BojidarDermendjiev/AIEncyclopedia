using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Domain.Entities;

public class Topic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Summary { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Intermediate;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Category Category { get; set; } = null!;
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<KnowledgeRelation> OutgoingRelations { get; set; } = new List<KnowledgeRelation>();
    public ICollection<KnowledgeRelation> IncomingRelations { get; set; } = new List<KnowledgeRelation>();
}
