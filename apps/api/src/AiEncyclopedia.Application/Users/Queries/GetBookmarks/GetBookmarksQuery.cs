using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Users.Queries.GetBookmarks;

public record BookmarkDto(Guid ArticleId, string Title, string Slug, string? Summary, int ReadingTimeMinutes, DateTimeOffset BookmarkedAt);

public record GetBookmarksQuery(Guid UserId) : IRequest<ApiResponse<IReadOnlyList<BookmarkDto>>>;

public class GetBookmarksQueryHandler : IRequestHandler<GetBookmarksQuery, ApiResponse<IReadOnlyList<BookmarkDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetBookmarksQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<BookmarkDto>>> Handle(GetBookmarksQuery request, CancellationToken ct)
    {
        var items = await _db.UserBookmarks
            .Where(b => b.UserId == request.UserId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookmarkDto(
                b.ArticleId, b.Article.Title, b.Article.Slug,
                b.Article.Summary, b.Article.ReadingTimeMinutes, b.CreatedAt))
            .ToListAsync(ct);

        return ApiResponse<IReadOnlyList<BookmarkDto>>.Ok(items);
    }
}
