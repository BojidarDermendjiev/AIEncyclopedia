using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Enums;
using AiEncyclopedia.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace AiEncyclopedia.Infrastructure.Payment;

public class StripeService : IStripeService
{
    private readonly StripeOptions _opts;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IOptions<StripeOptions> opts, ILogger<StripeService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task<string> CreateCheckoutSessionAsync(string customerEmail, SubscriptionTier tier, string successUrl, string cancelUrl, CancellationToken ct = default)
    {
        var priceId = tier == SubscriptionTier.Enterprise
            ? _opts.EnterprisePriceId
            : _opts.ProPriceId;

        var options = new SessionCreateOptions
        {
            CustomerEmail = customerEmail,
            Mode = "subscription",
            LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string> { ["tier"] = tier.ToString() }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);
        return session.Url;
    }

    public async Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct = default)
    {
        var options = new global::Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = stripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new global::Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);
        return session.Url;
    }

    public Task<WebhookResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _opts.WebhookSecret);
            return Task.FromResult(ParseEvent(stripeEvent));
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Stripe webhook signature validation failed: {Message}", ex.Message);
            return Task.FromResult(new WebhookResult(false, ex.Message));
        }
    }

    private static WebhookResult ParseEvent(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case EventTypes.CustomerSubscriptionUpdated:
            case EventTypes.CustomerSubscriptionCreated:
            {
                var sub = stripeEvent.Data.Object as global::Stripe.Subscription;
                if (sub == null) return new WebhookResult(true);
                return new WebhookResult(true,
                    CustomerId: sub.CustomerId,
                    Tier: ParseTier(sub.Metadata?.GetValueOrDefault("tier")),
                    Status: ParseStatus(sub.Status),
                    PeriodEnd: sub.CurrentPeriodEnd);
            }
            case EventTypes.CustomerSubscriptionDeleted:
            {
                var sub = stripeEvent.Data.Object as global::Stripe.Subscription;
                if (sub == null) return new WebhookResult(true);
                return new WebhookResult(true,
                    CustomerId: sub.CustomerId,
                    Tier: SubscriptionTier.Free,
                    Status: SubscriptionStatus.Canceled);
            }
            case EventTypes.InvoicePaid:
            {
                var invoice = stripeEvent.Data.Object as Invoice;
                if (invoice == null) return new WebhookResult(true);
                return new WebhookResult(true,
                    CustomerId: invoice.CustomerId,
                    Tier: ParseTier(invoice.Metadata?.GetValueOrDefault("tier")),
                    Status: SubscriptionStatus.Active);
            }
            default:
                return new WebhookResult(true);
        }
    }

    private static SubscriptionTier ParseTier(string? tier) => tier?.ToLowerInvariant() switch
    {
        "enterprise" => SubscriptionTier.Enterprise,
        "pro" => SubscriptionTier.Pro,
        _ => SubscriptionTier.Free
    };

    private static SubscriptionStatus ParseStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "active" => SubscriptionStatus.Active,
        "past_due" => SubscriptionStatus.PastDue,
        "canceled" or "cancelled" => SubscriptionStatus.Canceled,
        _ => SubscriptionStatus.Active
    };
}
