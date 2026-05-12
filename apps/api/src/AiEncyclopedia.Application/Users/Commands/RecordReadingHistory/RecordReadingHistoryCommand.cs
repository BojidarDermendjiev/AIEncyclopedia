using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Users.Commands.RecordReadingHistory;

public record RecordReadingHistoryCommand(Guid UserId, Guid ArticleId, int ProgressPercent) : IRequest<ApiResponse<bool>>;

public class RecordReadingHistoryCommandHandler : IRequestHandler<RecordReadingHistoryCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _db;

    public RecordReadingHistoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<bool>> Handle(RecordReadingHistoryCommand request, CancellationToken ct)
    {
        var entry = await _db.ReadingHistories
            .FirstOrDefaultAsync(h => h.UserId == request.UserId && h.ArticleId == request.ArticleId, ct);

        if (entry == null)
        {
            _db.ReadingHistories.Add(new ReadingHistory
            {
                UserId = request.UserId,
                ArticleId = request.ArticleId,
                ProgressPercent = Math.Clamp(request.ProgressPercent, 0, 100),
                LastReadAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            entry.ProgressPercent = Math.Clamp(request.ProgressPercent, 0, 100);
            entry.LastReadAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true);
    }
}
