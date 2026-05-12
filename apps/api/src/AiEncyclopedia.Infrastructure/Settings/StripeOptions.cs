namespace AiEncyclopedia.Infrastructure.Settings;

public sealed class StripeOptions
{
    public const string Section = "Stripe";

    public string SecretKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string ProPriceId { get; init; } = string.Empty;
    public string EnterprisePriceId { get; init; } = string.Empty;
}
