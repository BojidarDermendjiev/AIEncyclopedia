using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Articles.Commands.GenerateArticleAI;

public record GenerateArticleAICommand(
    Guid TopicId,
    DifficultyLevel Difficulty,
    AIProviderName? Provider = null
) : IRequest<ApiResponse<Guid>>;

public class GenerateArticleAICommandValidator : AbstractValidator<GenerateArticleAICommand>
{
    public GenerateArticleAICommandValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
    }
}

public class GenerateArticleAICommandHandler : IRequestHandler<GenerateArticleAICommand, ApiResponse<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAIProviderFactory _aiFactory;

    public GenerateArticleAICommandHandler(IApplicationDbContext db, IAIProviderFactory aiFactory)
    {
        _db = db;
        _aiFactory = aiFactory;
    }

    public async Task<ApiResponse<Guid>> Handle(GenerateArticleAICommand request, CancellationToken ct)
    {
        var topic = await _db.Topics.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == request.TopicId, ct);
        if (topic == null)
            return ApiResponse<Guid>.Fail("Topic not found.");

        var provider = _aiFactory.GetProvider(request.Provider);

        var job = new AIJob
        {
            JobType = AIJobType.GenerateArticle,
            Status = AIJobStatus.Processing,
            Provider = provider.ProviderType,
            InputData = System.Text.Json.JsonSerializer.Serialize(new { TopicId = request.TopicId, Difficulty = request.Difficulty.ToString() }),
            StartedAt = DateTimeOffset.UtcNow
        };
        _db.AIJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        try
        {
            var outline = await provider.GenerateOutlineAsync(topic.Title, request.Difficulty, ct);
            var outlineText = $"{outline.Title}\n" + string.Join("\n", outline.Sections.Select((s, i) => $"{i + 1}. {s}"));
            var genReq = new ArticleGenerationRequest(topic.Title, outlineText, request.Difficulty);
            var content = await provider.GenerateArticleAsync(genReq, ct);
            var tags = await provider.GenerateTagsAsync(content, 8, ct);
            var seo = await provider.GenerateSeoMetadataAsync(topic.Title, content, ct);
            var summary = await provider.SummarizeAsync(content, 200, ct);

            var slug = GenerateSlug(topic.Title);
            var existing = await _db.Articles.AnyAsync(a => a.Slug == slug, ct);
            if (existing) slug = $"{slug}-{Guid.NewGuid():N[..6]}";

            var article = new Article
            {
                TopicId = topic.Id,
                Title = topic.Title,
                Slug = slug,
                Content = content,
                Summary = summary,
                Status = ArticleStatus.Draft,
                AiGenerated = true,
                AiProvider = provider.ProviderType,
                SeoTitle = seo.Title,
                SeoDescription = seo.Description,
                SeoKeywords = seo.Keywords,
                ReadingTimeMinutes = Math.Max(1, content.Split(' ').Length / 200)
            };
            _db.Articles.Add(article);

            foreach (var tagName in tags)
            {
                var normalized = tagName.Trim().ToLower();
                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == normalized, ct)
                    ?? new Tag { Name = normalized, Slug = GenerateSlug(normalized) };
                if (tag.Id == Guid.Empty)
                    _db.Tags.Add(tag);
                article.ArticleTags.Add(new ArticleTag { Article = article, Tag = tag });
            }

            job.Status = AIJobStatus.Done;
            job.OutputData = System.Text.Json.JsonSerializer.Serialize(new { ArticleId = article.Id });
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);

            return ApiResponse<Guid>.Ok(article.Id, "Article generated successfully.");
        }
        catch (Exception ex)
        {
            job.Status = AIJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return ApiResponse<Guid>.Fail($"AI generation failed: {ex.Message}");
        }
    }

    private static string GenerateSlug(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text.ToLower().Trim(), @"[^a-z0-9]+", "-").Trim('-');
}
