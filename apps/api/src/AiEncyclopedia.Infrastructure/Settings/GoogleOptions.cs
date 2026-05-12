namespace AiEncyclopedia.Infrastructure.Settings;

public sealed class GoogleOptions
{
    public const string Section = "Google";

    public string GeminiApiKey { get; init; } = string.Empty;
    public string GeminiModel { get; init; } = "gemini-1.5-pro";
}
