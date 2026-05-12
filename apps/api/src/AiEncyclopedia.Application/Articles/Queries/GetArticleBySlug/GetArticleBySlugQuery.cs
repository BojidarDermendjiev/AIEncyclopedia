using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Articles.Queries.GetArticleBySlug;

public record GetArticleBySlugQuery(string Slug) : IRequest<ApiResponse<Article>>;

public class GetArticleBySlugQueryHandler : IRequestHandler<GetArticleBySlugQuery, ApiResponse<Article>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;

    public GetArticleBySlugQueryHandler(IApplicationDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ApiResponse<Article>> Handle(GetArticleBySlugQuery request, CancellationToken ct)
    {
        var cacheKey = $"article:{request.Slug}";
        var cached = await _cache.GetAsync<Article>(cacheKey, ct);
        if (cached != null)
            return ApiResponse<Article>.Ok(cached);

        var article = await _db.Articles
            .Include(a => a.Topic).ThenInclude(t => t.Category)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Slug == request.Slug && a.Status == Domain.Enums.ArticleStatus.Published, ct);

        if (article == null)
            return ApiResponse<Article>.Fail("Article not found.");

        await _cache.SetAsync(cacheKey, article, TimeSpan.FromHours(1), ct);
        return ApiResponse<Article>.Ok(article);
    }
}
