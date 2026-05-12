using System.Text;
using System.Text.Json;
using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Enums;
using AiEncyclopedia.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiEncyclopedia.Infrastructure.AI;

public class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<ClaudeProvider> _logger;
    private readonly AnthropicOptions _opts;

    public AIProviderName ProviderType => AIProviderName.Claude;

    public ClaudeProvider(HttpClient http, IOptions<AnthropicOptions> opts, ILogger<ClaudeProvider> logger)
    {
        _http = http;
        _logger = logger;
        _opts = opts.Value;
        _http.DefaultRequestHeaders.Add("x-api-key", _opts.ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", _opts.ApiVersion);
    }

    public async Task<string> GenerateArticleAsync(ArticleGenerationRequest request, CancellationToken ct = default)
    {
        var prompt = $"""
            Write a comprehensive educational article about "{request.Topic}".
            Difficulty level: {request.DifficultyLevel}.
            Target length: approximately {request.TargetWordCount} words.
            {(request.Outline != null ? $"Follow this outline:\n{request.Outline}" : "")}
            Format in Markdown. Include sections, definitions, examples, and a summary.
            """;
        return await ChatAsync(prompt, ct);
    }

    public async Task<string> SummarizeAsync(string content, int maxWords = 200, CancellationToken ct = default)
        => await ChatAsync($"Summarize the following in {maxWords} words or fewer:\n\n{content}", ct);

    public async Task<OutlineResult> GenerateOutlineAsync(string topic, DifficultyLevel difficulty, CancellationToken ct = default)
    {
        var json = await ChatAsync(
            $"Return only valid JSON (no markdown): {{\"title\": \"...\", \"sections\": [\"...\"]}} for an educational article outline about \"{topic}\" at {difficulty} level.",
            ct);
        return ParseOutline(json, topic);
    }

    public async Task<string[]> GenerateTagsAsync(string content, int maxTags = 10, CancellationToken ct = default)
    {
        var result = await ChatAsync(
            $"Return only a JSON array of {maxTags} tags for this content (no markdown): [\"tag1\",...]\n\n{content[..Math.Min(2000, content.Length)]}",
            ct);
        return ParseStringArray(result);
    }

    public async Task<SeoMetadata> GenerateSeoMetadataAsync(string title, string content, CancellationToken ct = default)
    {
        var result = await ChatAsync(
            $"Return only valid JSON for SEO metadata for \"{title}\" (no markdown): {{\"title\":\"\",\"description\":\"\",\"keywords\":[]}}",
            ct);
        return ParseSeoMetadata(result, title);
    }

    public async Task<bool> ValidateContentQualityAsync(string content, CancellationToken ct = default)
    {
        var result = await ChatAsync(
            $"Is this content educational and suitable for publication? Answer only 'yes' or 'no'.\n\n{content[..Math.Min(1000, content.Length)]}",
            ct);
        return result.Trim().StartsWith("yes", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> ChatAsync(string userMessage, CancellationToken ct)
    {
        var body = new
        {
            model = _opts.Model,
            max_tokens = _opts.MaxTokens,
            messages = new[] { new { role = "user", content = userMessage } }
        };
        var response = await _http.PostAsync("messages",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"), ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Claude request failed {Status}: {Body}", response.StatusCode, err);
            response.EnsureSuccessStatusCode();
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
    }

    private static OutlineResult ParseOutline(string json, string fallbackTitle)
    {
        var doc = JsonDocument.Parse(json.Trim());
        return new OutlineResult(
            doc.RootElement.GetProperty("title").GetString() ?? fallbackTitle,
            doc.RootElement.GetProperty("sections").EnumerateArray().Select(e => e.GetString() ?? "").ToArray());
    }

    private static string[] ParseStringArray(string json)
    {
        try { return JsonSerializer.Deserialize<string[]>(json.Trim()) ?? []; }
        catch { return []; }
    }

    private static SeoMetadata ParseSeoMetadata(string json, string fallbackTitle)
    {
        var doc = JsonDocument.Parse(json.Trim());
        return new SeoMetadata(
            doc.RootElement.GetProperty("title").GetString() ?? fallbackTitle,
            doc.RootElement.GetProperty("description").GetString() ?? "",
            doc.RootElement.GetProperty("keywords").EnumerateArray().Select(e => e.GetString() ?? "").ToArray());
    }
}
