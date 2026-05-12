using AiEncyclopedia.Application.Pdfs.Commands.GeneratePdf;
using AiEncyclopedia.Application.Pdfs.Queries.GetPdfBytes;
using AiEncyclopedia.Application.Users.Queries.GetUserPdfs;
using AiEncyclopedia.Domain.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AiEncyclopedia.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("api")]
public class PdfsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PdfsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("generate")]
    [Authorize]
    public async Task<IActionResult> Generate(
        [FromBody] GeneratePdfRequest req,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GeneratePdfCommand(req.ArticleId, req.Style, userId), ct);
        if (!result.Success)
            return BadRequest(result);

        // Return download URL instead of bytes in the body
        return Ok(new { result.Data!.Id, result.Data.FileSizeBytes, result.Data.ExportStyle, result.Data.GeneratedAt });
    }

    [HttpGet("{id}/download")]
    [Authorize]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var pdf = await _mediator.Send(new GetPdfBytesQuery(id, userId), ct);
        if (!pdf.Success || pdf.Data == null)
            return NotFound(pdf);

        var cd = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        {
            FileName = $"article-{id}.pdf"
        };
        Response.Headers.ContentDisposition = cd.ToString();
        return File(pdf.Data, "application/pdf");
    }

    [HttpGet("library")]
    [Authorize]
    public async Task<IActionResult> GetLibrary(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new GetUserPdfsQuery(userId.Value), ct);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idStr, out var id) ? id : null;
    }
}

public record GeneratePdfRequest(Guid ArticleId, PdfExportStyle Style);
