using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Users.Queries.GetUserPdfs;

public record UserPdfDto(Guid Id, Guid ArticleId, string ArticleTitle, string ArticleSlug, PdfExportStyle Style, long FileSizeBytes, int DownloadCount, DateTimeOffset GeneratedAt);

public record GetUserPdfsQuery(Guid UserId) : IRequest<ApiResponse<IReadOnlyList<UserPdfDto>>>;

public class GetUserPdfsQueryHandler : IRequestHandler<GetUserPdfsQuery, ApiResponse<IReadOnlyList<UserPdfDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetUserPdfsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<UserPdfDto>>> Handle(GetUserPdfsQuery request, CancellationToken ct)
    {
        var items = await _db.PdfDocuments
            .Where(p => p.UserId == request.UserId)
            .OrderByDescending(p => p.GeneratedAt)
            .Select(p => new UserPdfDto(
                p.Id, p.ArticleId, p.Article.Title, p.Article.Slug,
                p.ExportStyle, p.FileSizeBytes, p.DownloadCount, p.GeneratedAt))
            .ToListAsync(ct);

        return ApiResponse<IReadOnlyList<UserPdfDto>>.Ok(items);
    }
}
