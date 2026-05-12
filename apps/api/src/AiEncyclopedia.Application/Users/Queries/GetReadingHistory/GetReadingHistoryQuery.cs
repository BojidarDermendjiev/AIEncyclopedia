using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Users.Queries.GetReadingHistory;

public record ReadingHistoryDto(Guid ArticleId, string Title, string Slug, int ProgressPercent, DateTimeOffset LastReadAt);

public record GetReadingHistoryQuery(Guid UserId, int Limit = 20) : IRequest<ApiResponse<IReadOnlyList<ReadingHistoryDto>>>;

public class GetReadingHistoryQueryHandler : IRequestHandler<GetReadingHistoryQuery, ApiResponse<IReadOnlyList<ReadingHistoryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetReadingHistoryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<ReadingHistoryDto>>> Handle(GetReadingHistoryQuery request, CancellationToken ct)
    {
        var items = await _db.ReadingHistories
            .Where(h => h.UserId == request.UserId)
            .OrderByDescending(h => h.LastReadAt)
            .Take(request.Limit)
            .Select(h => new ReadingHistoryDto(
                h.ArticleId, h.Article.Title, h.Article.Slug,
                h.ProgressPercent, h.LastReadAt))
            .ToListAsync(ct);

        return ApiResponse<IReadOnlyList<ReadingHistoryDto>>.Ok(items);
    }
}
