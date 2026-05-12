using System.ComponentModel.DataAnnotations;

namespace AiEncyclopedia.Infrastructure.Settings;

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    [Required(AllowEmptyStrings = false)]
    public string Secret { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Audience { get; init; } = string.Empty;

    [Range(5, 1440)]
    public int ExpiryMinutes { get; init; } = 60;

    [Range(1, 365)]
    public int RefreshExpiryDays { get; init; } = 30;
}
