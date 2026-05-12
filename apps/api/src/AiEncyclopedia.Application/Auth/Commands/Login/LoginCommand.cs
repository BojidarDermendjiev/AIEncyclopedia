using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainRefreshToken = AiEncyclopedia.Domain.Entities.RefreshToken;

namespace AiEncyclopedia.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<ApiResponse<TokenPair>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<TokenPair>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<TokenPair>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && u.IsActive, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<TokenPair>.Fail("Invalid email or password.");

        var tokens = _tokenService.GenerateTokenPair(user);
        var refreshToken = new DomainRefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<TokenPair>.Ok(tokens);
    }
}
