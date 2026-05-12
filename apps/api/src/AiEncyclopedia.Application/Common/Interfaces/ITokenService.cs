using AiEncyclopedia.Domain.Entities;

namespace AiEncyclopedia.Application.Common.Interfaces;

public record TokenPair(string AccessToken, string RefreshToken, int ExpiresInSeconds);

public interface ITokenService
{
    TokenPair GenerateTokenPair(User user);
    string? GetUserIdFromExpiredToken(string token);
}
