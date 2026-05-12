namespace AiEncyclopedia.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Color { get; set; }

    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
