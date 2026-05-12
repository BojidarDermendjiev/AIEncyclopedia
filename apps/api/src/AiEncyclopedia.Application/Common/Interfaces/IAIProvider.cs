using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Application.Common.Interfaces;

public record ArticleGenerationRequest(
    string Topic,
    string? Outline,
    DifficultyLevel DifficultyLevel,
    int TargetWordCount = 1500);

public record OutlineResult(string Title, string[] Sections);

public record SeoMetadata(string Title, string Description, string[] Keywords);

public interface IAIProvider
{
    AIProviderName ProviderType { get; }
    Task<string> GenerateArticleAsync(ArticleGenerationRequest request, CancellationToken ct = default);
    Task<string> SummarizeAsync(string content, int maxWords = 200, CancellationToken ct = default);
    Task<OutlineResult> GenerateOutlineAsync(string topic, DifficultyLevel difficulty, CancellationToken ct = default);
    Task<string[]> GenerateTagsAsync(string content, int maxTags = 10, CancellationToken ct = default);
    Task<SeoMetadata> GenerateSeoMetadataAsync(string title, string content, CancellationToken ct = default);
    Task<bool> ValidateContentQualityAsync(string content, CancellationToken ct = default);
}

public interface IAIProviderFactory
{
    IAIProvider GetProvider(AIProviderName? provider = null);
    IAIProvider GetDefaultProvider();
}
