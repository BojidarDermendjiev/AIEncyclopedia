using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Application.Common.Interfaces;

public record CheckoutSession(string SessionId, string Url);
public record CustomerPortalSession(string Url);
public record WebhookResult(bool Success, string? Error = null, string? CustomerId = null, SubscriptionTier? Tier = null, SubscriptionStatus? Status = null, DateTimeOffset? PeriodEnd = null);

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(string customerEmail, SubscriptionTier tier, string successUrl, string cancelUrl, CancellationToken ct = default);
    Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct = default);
    Task<WebhookResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default);
}
