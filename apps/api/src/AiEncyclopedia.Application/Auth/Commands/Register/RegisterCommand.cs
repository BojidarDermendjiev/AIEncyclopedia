using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainRefreshToken = AiEncyclopedia.Domain.Entities.RefreshToken;

namespace AiEncyclopedia.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<ApiResponse<TokenPair>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.DisplayName).NotEmpty().MinimumLength(2).MaximumLength(100);
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<TokenPair>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<TokenPair>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct))
            return ApiResponse<TokenPair>.Fail("Email is already registered.");

        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            DisplayName = request.DisplayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);

        var tokens = _tokenService.GenerateTokenPair(user);
        var refreshToken = new DomainRefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync(ct);
        return ApiResponse<TokenPair>.Ok(tokens, "Registration successful.");
    }
}
