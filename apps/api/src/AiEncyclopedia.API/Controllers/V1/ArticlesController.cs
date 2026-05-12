using AiEncyclopedia.Application.Articles.Queries.GetArticleBySlug;
using AiEncyclopedia.Application.Articles.Queries.GetArticles;
using AiEncyclopedia.Domain.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AiEncyclopedia.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("api")]
public class ArticlesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ArticlesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetArticles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? topicId = null,
        [FromQuery] string? search = null,
        [FromQuery] DifficultyLevel? difficulty = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetArticlesQuery(page, Math.Clamp(pageSize, 1, 50), categoryId, topicId, search, difficulty), ct);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetArticleBySlugQuery(slug), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
