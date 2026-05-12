using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AiEncyclopedia.Infrastructure.Pdf;

public class PdfGenerationService : IPdfGenerationService
{
    public PdfGenerationService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateAsync(Article article, PdfExportStyle style, CancellationToken ct = default)
    {
        var bytes = style switch
        {
            PdfExportStyle.Academic => GenerateAcademic(article),
            PdfExportStyle.Student => GenerateStudent(article),
            PdfExportStyle.Presentation => GeneratePresentation(article),
            PdfExportStyle.Summary => GenerateSummary(article),
            _ => GenerateAcademic(article)
        };
        return Task.FromResult(bytes);
    }

    private static byte[] GenerateAcademic(Article article)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Times New Roman"));

                page.Header().Column(col =>
                {
                    col.Item().Text(article.Title).FontSize(18).Bold().AlignCenter();
                    col.Item().PaddingTop(4).Text($"Category: {article.Topic?.Category?.Name ?? "General"} | Reading time: {article.ReadingTimeMinutes} min").FontSize(9).FontColor(Colors.Grey.Darken1).AlignCenter();
                    if (article.PublishedAt.HasValue)
                        col.Item().Text($"Published: {article.PublishedAt:MMMM dd, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1).AlignCenter();
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Black);
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(article.Summary))
                    {
                        col.Item().Text("Abstract").FontSize(13).Bold();
                        col.Item().PaddingTop(4).PaddingBottom(12)
                            .Text(article.Summary).Italic().FontColor(Colors.Grey.Darken2);
                    }
                    col.Item().Text(StripHtml(article.Content)).FontSize(11).LineHeight(1.5f);
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static byte[] GenerateStudent(Article article)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                page.Header().Background(Colors.Blue.Lighten4).Padding(12).Column(col =>
                {
                    col.Item().Text(article.Title).FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(4).Text($"{article.Topic?.Category?.Name ?? "General"} · {article.ReadingTimeMinutes} min read").FontSize(10).FontColor(Colors.Blue.Darken1);
                });

                page.Content().PaddingTop(16).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(article.Summary))
                    {
                        col.Item().Background(Colors.Yellow.Lighten4).Padding(10).Text($"Key Takeaway: {article.Summary}").Italic();
                        col.Item().PaddingBottom(12);
                    }
                    col.Item().Text(StripHtml(article.Content)).FontSize(12).LineHeight(1.6f);
                });

                page.Footer().AlignRight().PaddingRight(12).Text(text =>
                {
                    text.Span("Page ").FontSize(10);
                    text.CurrentPageNumber().FontSize(10);
                });
            });
        }).GeneratePdf();
    }

    private static byte[] GeneratePresentation(Article article)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(14).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Item().Background(Colors.Indigo.Darken2).Padding(20).Column(inner =>
                    {
                        inner.Item().Text(article.Title).FontSize(28).Bold().FontColor(Colors.White);
                        inner.Item().PaddingTop(8).Text(article.Topic?.Category?.Name ?? "General").FontSize(16).FontColor(Colors.Indigo.Lighten3);
                    });

                    col.Item().PaddingTop(20).Text(StripHtml(article.Summary ?? article.Content.Substring(0, Math.Min(600, article.Content.Length))))
                        .FontSize(14).LineHeight(1.5f);

                    if (article.ArticleTags.Count > 0)
                    {
                        col.Item().PaddingTop(16).Row(row =>
                        {
                            foreach (var tag in article.ArticleTags.Take(6))
                                row.AutoItem().Background(Colors.Indigo.Lighten4).Padding(4).PaddingHorizontal(8).Text(tag.Tag?.Name ?? "").FontSize(11).FontColor(Colors.Indigo.Darken3);
                        });
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.CurrentPageNumber().FontSize(11);
                    text.Span(" / ").FontSize(11);
                    text.TotalPages().FontSize(11);
                });
            });
        }).GeneratePdf();
    }

    private static byte[] GenerateSummary(Article article)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Item().Text(article.Title).FontSize(16).Bold();
                    col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(8).Text(StripHtml(article.Summary ?? article.Content.Substring(0, Math.Min(800, article.Content.Length))))
                        .FontSize(11).LineHeight(1.5f);

                    if (article.ArticleTags.Count > 0)
                    {
                        col.Item().PaddingTop(12).Text("Tags: " + string.Join(", ", article.ArticleTags.Select(t => t.Tag?.Name ?? ""))).FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                });
            });
        }).GeneratePdf();
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ")
            .Replace("&nbsp;", " ").Replace("&amp;", "&").Replace("&lt;", "<")
            .Replace("&gt;", ">").Replace("&quot;", "\"")
            .Trim();
    }
}
