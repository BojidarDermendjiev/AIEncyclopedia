using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiEncyclopedia.Application.Subscriptions.Commands.HandleWebhook;

public record HandleWebhookCommand(string Payload, string StripeSignature) : IRequest<ApiResponse<bool>>;

public class HandleWebhookCommandHandler : IRequestHandler<HandleWebhookCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly IStripeService _stripe;
    private readonly ILogger<HandleWebhookCommandHandler> _logger;

    public HandleWebhookCommandHandler(IApplicationDbContext db, IStripeService stripe, ILogger<HandleWebhookCommandHandler> logger)
    {
        _db = db;
        _stripe = stripe;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(HandleWebhookCommand request, CancellationToken ct)
    {
        var result = await _stripe.ProcessWebhookAsync(request.Payload, request.StripeSignature, ct);
        if (!result.Success)
            return ApiResponse<bool>.Fail(result.Error ?? "Webhook processing failed.");

        if (result.CustomerId != null && result.Tier.HasValue)
        {
            var sub = await _db.Subscriptions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StripeCustomerId == result.CustomerId, ct);

            if (sub != null)
            {
                sub.Tier = result.Tier.Value;
                sub.Status = result.Status ?? Domain.Enums.SubscriptionStatus.Active;
                sub.CurrentPeriodEnd = result.PeriodEnd ?? sub.CurrentPeriodEnd;
                if (sub.User != null)
                    sub.User.SubscriptionTier = result.Tier.Value;
                await _db.SaveChangesAsync(ct);
                _logger.LogInformation("Updated subscription for customer {CustomerId} to {Tier}", result.CustomerId, result.Tier);
            }
        }

        return ApiResponse<bool>.Ok(true);
    }
}
