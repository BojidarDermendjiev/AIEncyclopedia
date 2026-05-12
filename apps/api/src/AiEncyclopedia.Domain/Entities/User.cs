using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public Subscription? Subscription { get; set; }
    public ICollection<UserBookmark> Bookmarks { get; set; } = new List<UserBookmark>();
    public ICollection<ReadingHistory> ReadingHistory { get; set; } = new List<ReadingHistory>();
}
