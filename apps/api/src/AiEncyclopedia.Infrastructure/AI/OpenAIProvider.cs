using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Enums;
using AiEncyclopedia.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiEncyclopedia.Infrastructure.AI;

public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenAIProvider> _logger;
    private readonly OpenAIOptions _opts;

    public AIProviderName ProviderType => AIProviderName.OpenAI;

    public OpenAIProvider(HttpClient http, IOptions<OpenAIOptions> opts, ILogger<OpenAIProvider> logger)
    {
        _http = http;
        _logger = logger;
        _opts = opts.Value;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _opts.ApiKey);
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
        => await ChatAsync($"Summarize the following content in {maxWords} words or fewer:\n\n{content}", ct);

    public async Task<OutlineResult> GenerateOutlineAsync(string topic, DifficultyLevel difficulty, CancellationToken ct = default)
    {
        var json = await ChatAsync(
            $"Generate a JSON outline for an educational article about \"{topic}\" at {difficulty} level. " +
            "Return only valid JSON with no markdown fences: {\"title\": \"...\", \"sections\": [\"Section 1\", ...]}",
            ct);
        return ParseOutline(json, topic);
    }

    public async Task<string[]> GenerateTagsAsync(string content, int maxTags = 10, CancellationToken ct = default)
    {
        var result = await ChatAsync(
            $"Extract {maxTags} relevant tags from this content. Return only a JSON array of strings with no markdown fences: [\"tag1\", \"tag2\", ...]\n\n{content[..Math.Min(2000, content.Length)]}",
            ct);
        return ParseStringArray(result);
    }

    public async Task<SeoMetadata> GenerateSeoMetadataAsync(string title, string content, CancellationToken ct = default)
    {
        var result = await ChatAsync(
            $"Generate SEO metadata for an article titled \"{title}\". " +
            "Return only valid JSON with no markdown fences: {\"title\": \"max 70 chars\", \"description\": \"max 160 chars\", \"keywords\": [\"kw1\", ...]}",
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
            messages = new[] { new { role = "user", content = userMessage } },
            max_tokens = _opts.MaxTokens,
            temperature = _opts.Temperature
        };
        var response = await _http.PostAsync("chat/completions",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"), ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("OpenAI request failed {Status}: {Body}", response.StatusCode, err);
            response.EnsureSuccessStatusCode();
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }

    private static OutlineResult ParseOutline(string json, string fallbackTitle)
    {
        var cleaned = CleanJson(json);
        var doc = JsonDocument.Parse(cleaned);
        return new OutlineResult(
            doc.RootElement.GetProperty("title").GetString() ?? fallbackTitle,
            doc.RootElement.GetProperty("sections").EnumerateArray().Select(e => e.GetString() ?? "").ToArray());
    }

    private static string[] ParseStringArray(string json)
    {
        try { return JsonSerializer.Deserialize<string[]>(CleanJson(json)) ?? []; }
        catch { return []; }
    }

    private static SeoMetadata ParseSeoMetadata(string json, string fallbackTitle)
    {
        var doc = JsonDocument.Parse(CleanJson(json));
        return new SeoMetadata(
            doc.RootElement.GetProperty("title").GetString() ?? fallbackTitle,
            doc.RootElement.GetProperty("description").GetString() ?? "",
            doc.RootElement.GetProperty("keywords").EnumerateArray().Select(e => e.GetString() ?? "").ToArray());
    }

    private static string CleanJson(string raw) =>
        raw.Trim().TrimStart('`').TrimStart('j').TrimStart('s').TrimStart('o').TrimStart('n').TrimStart('\n').Trim('`');
}
