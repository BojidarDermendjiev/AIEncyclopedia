namespace AiEncyclopedia.Infrastructure.Settings;

public sealed class AIOptions
{
    public const string Section = "AI";

    public string DefaultProvider { get; init; } = "openai";
    public string FallbackProvider { get; init; } = "claude";
    public int MaxRetries { get; init; } = 2;
    public int TimeoutSeconds { get; init; } = 120;
}
