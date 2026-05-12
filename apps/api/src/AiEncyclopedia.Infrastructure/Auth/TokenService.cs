using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AiEncyclopedia.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwt;

    public TokenService(IOptions<JwtOptions> jwt) => _jwt = jwt.Value;

    public TokenPair GenerateTokenPair(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        return new TokenPair(accessToken, refreshToken, _jwt.ExpiryMinutes * 60);
    }

    public string? GetUserIdFromExpiredToken(string token)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim("subscription_tier", user.SubscriptionTier.ToString()),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : user.SubscriptionTier.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(_jwt.Secret);
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };
        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
        }
        catch { return null; }
    }
}
