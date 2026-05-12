using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Users.Commands.RemoveBookmark;

public record RemoveBookmarkCommand(Guid UserId, Guid ArticleId) : IRequest<ApiResponse<bool>>;

public class RemoveBookmarkCommandHandler : IRequestHandler<RemoveBookmarkCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _db;

    public RemoveBookmarkCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<bool>> Handle(RemoveBookmarkCommand request, CancellationToken ct)
    {
        var bookmark = await _db.UserBookmarks
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.ArticleId == request.ArticleId, ct);

        if (bookmark == null)
            return ApiResponse<bool>.Fail("Bookmark not found.");

        _db.UserBookmarks.Remove(bookmark);
        await _db.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true);
    }
}
