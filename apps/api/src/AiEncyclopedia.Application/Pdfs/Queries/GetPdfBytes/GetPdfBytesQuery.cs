using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Pdfs.Queries.GetPdfBytes;

public record GetPdfBytesQuery(Guid PdfId, Guid? UserId) : IRequest<ApiResponse<byte[]>>;

public class GetPdfBytesQueryHandler : IRequestHandler<GetPdfBytesQuery, ApiResponse<byte[]>>
{
    private readonly IApplicationDbContext _db;

    public GetPdfBytesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<byte[]>> Handle(GetPdfBytesQuery request, CancellationToken ct)
    {
        var pdf = await _db.PdfDocuments.FirstOrDefaultAsync(p => p.Id == request.PdfId, ct);
        if (pdf == null)
            return ApiResponse<byte[]>.Fail("PDF not found.");

        // Only the owner or admins can download; UserId null means admin
        if (request.UserId.HasValue && pdf.UserId.HasValue && pdf.UserId != request.UserId)
            return ApiResponse<byte[]>.Fail("Access denied.");

        pdf.DownloadCount++;
        await _db.SaveChangesAsync(ct);

        return ApiResponse<byte[]>.Ok(pdf.PdfBytes);
    }
}
