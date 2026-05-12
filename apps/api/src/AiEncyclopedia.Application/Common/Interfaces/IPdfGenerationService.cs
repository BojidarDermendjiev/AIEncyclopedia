using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Application.Common.Interfaces;

public interface IPdfGenerationService
{
    Task<byte[]> GenerateAsync(Article article, PdfExportStyle style, CancellationToken ct = default);
}
