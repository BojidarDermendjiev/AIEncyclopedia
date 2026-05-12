using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Pdfs.Commands.GeneratePdf;

public record GeneratePdfCommand(Guid ArticleId, PdfExportStyle Style, Guid? UserId) : IRequest<ApiResponse<PdfDocument>>;

public class GeneratePdfCommandValidator : AbstractValidator<GeneratePdfCommand>
{
    public GeneratePdfCommandValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();
    }
}

public class GeneratePdfCommandHandler : IRequestHandler<GeneratePdfCommand, ApiResponse<PdfDocument>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPdfGenerationService _pdfService;
    private readonly ICacheService _cache;

    public GeneratePdfCommandHandler(IApplicationDbContext db, IPdfGenerationService pdfService, ICacheService cache)
    {
        _db = db;
        _pdfService = pdfService;
        _cache = cache;
    }

    public async Task<ApiResponse<PdfDocument>> Handle(GeneratePdfCommand request, CancellationToken ct)
    {
        var article = await _db.Articles
            .Include(a => a.Topic).ThenInclude(t => t.Category)
            .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId && a.Status == ArticleStatus.Published, ct);

        if (article == null)
            return ApiResponse<PdfDocument>.Fail("Article not found or not published.");

        // Check if PDF already exists for this article+style
        var existing = await _db.PdfDocuments
            .FirstOrDefaultAsync(p => p.ArticleId == request.ArticleId
                && p.ExportStyle == request.Style
                && p.UserId == request.UserId, ct);
        if (existing != null)
            return ApiResponse<PdfDocument>.Ok(existing);

        var bytes = await _pdfService.GenerateAsync(article, request.Style, ct);

        var doc = new PdfDocument
        {
            ArticleId = article.Id,
            UserId = request.UserId,
            ExportStyle = request.Style,
            FileSizeBytes = bytes.Length,
            PdfBytes = bytes,
            GeneratedAt = DateTimeOffset.UtcNow
        };
        _db.PdfDocuments.Add(doc);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<PdfDocument>.Ok(doc);
    }
}
