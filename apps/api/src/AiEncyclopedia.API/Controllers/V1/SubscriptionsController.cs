using AiEncyclopedia.Application.Subscriptions.Commands.CreateCheckoutSession;
using AiEncyclopedia.Application.Subscriptions.Commands.HandleWebhook;
using AiEncyclopedia.Domain.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AiEncyclopedia.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("api")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SubscriptionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest req, CancellationToken ct)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var userId)) return Unauthorized();

        var result = await _mediator.Send(
            new CreateCheckoutSessionCommand(userId, req.Tier, req.SuccessUrl, req.CancelUrl), ct);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _mediator.Send(new HandleWebhookCommand(payload, signature), ct);
        return result.Success ? Ok() : BadRequest(result.Message);
    }
}

public record CheckoutRequest(SubscriptionTier Tier, string SuccessUrl, string CancelUrl);
