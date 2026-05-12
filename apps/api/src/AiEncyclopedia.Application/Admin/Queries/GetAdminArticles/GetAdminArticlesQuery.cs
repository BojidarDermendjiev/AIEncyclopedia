using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Admin.Queries.GetAdminArticles;

public record AdminArticleDto(Guid Id, string Title, string Slug, ArticleStatus Status, bool AiGenerated, AIProviderName? AiProvider, long ViewCount, DateTimeOffset CreatedAt, DateTimeOffset? PublishedAt);

public record GetAdminArticlesQuery(int Page = 1, int PageSize = 50, ArticleStatus? Status = null) : IRequest<ApiResponse<PaginatedResult<AdminArticleDto>>>;

public class GetAdminArticlesQueryHandler : IRequestHandler<GetAdminArticlesQuery, ApiResponse<PaginatedResult<AdminArticleDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAdminArticlesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PaginatedResult<AdminArticleDto>>> Handle(GetAdminArticlesQuery request, CancellationToken ct)
    {
        var query = _db.Articles.AsQueryable();
        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AdminArticleDto(a.Id, a.Title, a.Slug, a.Status, a.AiGenerated, a.AiProvider, a.ViewCount, a.CreatedAt, a.PublishedAt))
            .ToListAsync(ct);

        return ApiResponse<PaginatedResult<AdminArticleDto>>.Ok(new PaginatedResult<AdminArticleDto>(items, total, request.Page, request.PageSize));
    }
}
