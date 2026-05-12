using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Domain.Entities;

public class PdfDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArticleId { get; set; }
    public Guid? UserId { get; set; }
    public byte[] PdfBytes { get; set; } = Array.Empty<byte>();
    public long FileSizeBytes { get; set; }
    public PdfExportStyle ExportStyle { get; set; } = PdfExportStyle.Academic;
    public int DownloadCount { get; set; }
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    public Article Article { get; set; } = null!;
    public User? User { get; set; }
}
