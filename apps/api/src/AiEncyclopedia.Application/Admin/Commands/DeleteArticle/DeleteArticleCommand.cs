using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Admin.Commands.DeleteArticle;

public record DeleteArticleCommand(Guid ArticleId) : IRequest<ApiResponse<bool>>;

public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;

    public DeleteArticleCommandHandler(IApplicationDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteArticleCommand request, CancellationToken ct)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == request.ArticleId, ct);
        if (article == null)
            return ApiResponse<bool>.Fail("Article not found.");

        await _cache.RemoveAsync($"article:{article.Slug}", ct);
        _db.Articles.Remove(article);
        await _db.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true);
    }
}
