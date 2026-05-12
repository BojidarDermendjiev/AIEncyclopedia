using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Articles.Queries.SearchArticles;

public record SearchArticlesQuery(string Q, int Page = 1, int PageSize = 20) : IRequest<ApiResponse<PaginatedResult<SearchArticleDto>>>;

public record SearchArticleDto(
    Guid Id, string Title, string Slug, string? Summary,
    string TopicTitle, string CategoryName,
    int ReadingTimeMinutes, DateTimeOffset? PublishedAt, double Rank);

public class SearchArticlesQueryHandler : IRequestHandler<SearchArticlesQuery, ApiResponse<PaginatedResult<SearchArticleDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;

    public SearchArticlesQueryHandler(IApplicationDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ApiResponse<PaginatedResult<SearchArticleDto>>> Handle(SearchArticlesQuery request, CancellationToken ct)
    {
        var q = request.Q.Trim();
        if (string.IsNullOrEmpty(q))
            return ApiResponse<PaginatedResult<SearchArticleDto>>.Fail("Search query cannot be empty.");

        var cacheKey = $"search:{q.ToLower()}:{request.Page}:{request.PageSize}";
        var cached = await _cache.GetAsync<PaginatedResult<SearchArticleDto>>(cacheKey, ct);
        if (cached != null)
            return ApiResponse<PaginatedResult<SearchArticleDto>>.Ok(cached);

        var qLower = q.ToLower();
        var baseQuery = _db.Articles
            .Include(a => a.Topic).ThenInclude(t => t.Category)
            .Where(a => a.Status == Domain.Enums.ArticleStatus.Published
                && (a.Title.ToLower().Contains(qLower)
                    || (a.Summary != null && a.Summary.ToLower().Contains(qLower))
                    || a.Content.ToLower().Contains(qLower)));

        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderByDescending(a => a.PublishedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new SearchArticleDto(
                a.Id, a.Title, a.Slug, a.Summary,
                a.Topic.Title, a.Topic.Category.Name,
                a.ReadingTimeMinutes, a.PublishedAt, 1.0))
            .ToListAsync(ct);

        var result = new PaginatedResult<SearchArticleDto>(items, total, request.Page, request.PageSize);
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);
        return ApiResponse<PaginatedResult<SearchArticleDto>>.Ok(result);
    }
}
