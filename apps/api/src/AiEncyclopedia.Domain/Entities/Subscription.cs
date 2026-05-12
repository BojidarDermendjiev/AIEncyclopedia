using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public string StripeCustomerId { get; set; } = string.Empty;
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTimeOffset CurrentPeriodStart { get; set; }
    public DateTimeOffset CurrentPeriodEnd { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
}
