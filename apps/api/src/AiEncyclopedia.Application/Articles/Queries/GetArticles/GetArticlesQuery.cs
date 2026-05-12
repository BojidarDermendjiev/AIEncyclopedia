using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Articles.Queries.GetArticles;

public record GetArticlesQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    Guid? TopicId = null,
    string? Search = null,
    DifficultyLevel? Difficulty = null
) : IRequest<PaginatedResponse<Article>>;

public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PaginatedResponse<Article>>
{
    private readonly IApplicationDbContext _db;

    public GetArticlesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedResponse<Article>> Handle(GetArticlesQuery request, CancellationToken ct)
    {
        var query = _db.Articles
            .Include(a => a.Topic).ThenInclude(t => t.Category)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .Where(a => a.Status == ArticleStatus.Published)
            .AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(a => a.Topic.CategoryId == request.CategoryId.Value);

        if (request.TopicId.HasValue)
            query = query.Where(a => a.TopicId == request.TopicId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            query = query.Where(a => a.Title.ToLower().Contains(search) ||
                                     (a.Summary != null && a.Summary.ToLower().Contains(search)));
        }

        if (request.Difficulty.HasValue)
            query = query.Where(a => a.Topic.DifficultyLevel == request.Difficulty.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PaginatedResponse<Article>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
