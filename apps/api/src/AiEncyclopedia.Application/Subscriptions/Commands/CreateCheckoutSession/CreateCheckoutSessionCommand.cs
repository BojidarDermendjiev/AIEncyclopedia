using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Subscriptions.Commands.CreateCheckoutSession;

public record CreateCheckoutSessionCommand(Guid UserId, SubscriptionTier Tier, string SuccessUrl, string CancelUrl) : IRequest<ApiResponse<string>>;

public class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Tier).Must(t => t == SubscriptionTier.Pro || t == SubscriptionTier.Enterprise)
            .WithMessage("Only Pro and Enterprise tiers can be purchased.");
        RuleFor(x => x.SuccessUrl).NotEmpty();
        RuleFor(x => x.CancelUrl).NotEmpty();
    }
}

public class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _db;
    private readonly IStripeService _stripe;

    public CreateCheckoutSessionCommandHandler(IApplicationDbContext db, IStripeService stripe)
    {
        _db = db;
        _stripe = stripe;
    }

    public async Task<ApiResponse<string>> Handle(CreateCheckoutSessionCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user == null)
            return ApiResponse<string>.Fail("User not found.");

        var url = await _stripe.CreateCheckoutSessionAsync(user.Email, request.Tier, request.SuccessUrl, request.CancelUrl, ct);
        return ApiResponse<string>.Ok(url);
    }
}
