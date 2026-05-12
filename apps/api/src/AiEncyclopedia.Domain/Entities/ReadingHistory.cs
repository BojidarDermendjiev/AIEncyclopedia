namespace AiEncyclopedia.Domain.Entities;

public class ReadingHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ArticleId { get; set; }
    public int ProgressPercent { get; set; }
    public DateTimeOffset LastReadAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
    public Article Article { get; set; } = null!;
}
