using System.Text;
using System.Text.Json;
using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Enums;
using AiEncyclopedia.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiEncyclopedia.Infrastructure.AI;

public class GeminiProvider : IAIProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiProvider> _logger;
    private readonly GoogleOptions _opts;

    public AIProviderName ProviderType => AIProviderName.Gemini;

    public GeminiProvider(HttpClient http, IOptions<GoogleOptions> opts, ILogger<GeminiProvider> logger)
    {
        _http = http;
        _logger = logger;
        _opts = opts.Value;
    }

    public async Task<string> GenerateArticleAsync(ArticleGenerationRequest request, CancellationToken ct = default)
        => await GenerateAsync(
            $"Write a comprehensive educational article about \"{request.Topic}\" at {request.DifficultyLevel} level, " +
            $"approximately {request.TargetWordCount} words. " +
            $"{(request.Outline != null ? $"Follow this outline:\n{request.Outline}\n" : "")}" +
            "Format in Markdown. Include sections, definitions, examples, and a summary.",
            ct);

    public async Task<string> SummarizeAsync(string content, int maxWords = 200, CancellationToken ct = default)
        => await GenerateAsync($"Summarize in {maxWords} words:\n\n{content}", ct);

    public async Task<OutlineResult> GenerateOutlineAsync(string topic, DifficultyLevel difficulty, CancellationToken ct = default)
    {
        var json = await GenerateAsync(
            $"Return only valid JSON with no markdown fences: {{\"title\":\"...\",\"sections\":[...]}} for an outline about \"{topic}\" at {difficulty} level.",
            ct);
        return ParseOutline(json, topic);
    }

    public async Task<string[]> GenerateTagsAsync(string content, int maxTags = 10, CancellationToken ct = default)
    {
        var result = await GenerateAsync(
            $"Return only a JSON array of {maxTags} tags (no markdown fences): [\"tag1\",...]\n\n{content[..Math.Min(2000, content.Length)]}",
            ct);
        return ParseStringArray(result);
    }

    public async Task<SeoMetadata> GenerateSeoMetadataAsync(string title, string content, CancellationToken ct = default)
    {
        var result = await GenerateAsync(
            $"Return only valid JSON SEO metadata for \"{title}\" (no markdown fences): {{\"title\":\"\",\"description\":\"\",\"keywords\":[]}}",
            ct);
        return ParseSeoMetadata(result, title);
    }

    public async Task<bool> ValidateContentQualityAsync(string content, CancellationToken ct = default)
    {
        var result = await GenerateAsync(
            $"Is this content educational and suitable for publication? Answer only 'yes' or 'no'.\n\n{content[..Math.Min(1000, content.Length)]}",
            ct);
        return result.Trim().StartsWith("yes", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> GenerateAsync(string prompt, CancellationToken ct)
    {
        var url = $"models/{_opts.GeminiModel}:generateContent?key={_opts.GeminiApiKey}";
        var body = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
        var response = await _http.PostAsync(url,
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"), ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Gemini request failed {Status}: {Body}", response.StatusCode, err);
            response.EnsureSuccessStatusCode();
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";
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
