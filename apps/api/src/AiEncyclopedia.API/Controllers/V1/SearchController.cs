using AiEncyclopedia.Application.Articles.Queries.SearchArticles;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AiEncyclopedia.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("api")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;
    public SearchController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchArticlesQuery(q ?? "", page, Math.Clamp(pageSize, 1, 50)), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
