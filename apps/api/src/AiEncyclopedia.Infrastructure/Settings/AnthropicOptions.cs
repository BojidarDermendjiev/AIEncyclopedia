namespace AiEncyclopedia.Infrastructure.Settings;

public sealed class AnthropicOptions
{
    public const string Section = "Anthropic";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "claude-opus-4-7";
    public int MaxTokens { get; init; } = 4096;
    public string ApiVersion { get; init; } = "2023-06-01";
}
