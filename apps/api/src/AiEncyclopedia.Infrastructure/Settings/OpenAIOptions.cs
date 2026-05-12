namespace AiEncyclopedia.Infrastructure.Settings;

public sealed class OpenAIOptions
{
    public const string Section = "OpenAI";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-4o";
    public int MaxTokens { get; init; } = 4096;
    public double Temperature { get; init; } = 0.7;
}
