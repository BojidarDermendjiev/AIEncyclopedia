using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainRefreshToken = AiEncyclopedia.Domain.Entities.RefreshToken;

namespace AiEncyclopedia.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : IRequest<ApiResponse<TokenPair>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.Token).NotEmpty();
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<TokenPair>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<TokenPair>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.Token, ct);

        if (existing == null || !existing.IsActive)
            return ApiResponse<TokenPair>.Fail("Invalid or expired refresh token.");

        var newTokens = _tokenService.GenerateTokenPair(existing.User);

        existing.RevokedAt = DateTimeOffset.UtcNow;
        existing.ReplacedByToken = newTokens.RefreshToken;

        var newRefreshToken = new DomainRefreshToken
        {
            UserId = existing.UserId,
            Token = newTokens.RefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<TokenPair>.Ok(newTokens);
    }
}
