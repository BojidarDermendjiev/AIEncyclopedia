using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Admin.Commands.PublishArticle;

public record PublishArticleCommand(Guid ArticleId) : IRequest<ApiResponse<bool>>;

public class PublishArticleCommandHandler : IRequestHandler<PublishArticleCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;

    public PublishArticleCommandHandler(IApplicationDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ApiResponse<bool>> Handle(PublishArticleCommand request, CancellationToken ct)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == request.ArticleId, ct);
        if (article == null)
            return ApiResponse<bool>.Fail("Article not found.");

        article.Status = ArticleStatus.Published;
        article.PublishedAt ??= DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _cache.RemoveAsync($"article:{article.Slug}", ct);
        return ApiResponse<bool>.Ok(true);
    }
}
