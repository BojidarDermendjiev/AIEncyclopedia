using AiEncyclopedia.Application.Admin.Commands.DeleteArticle;
using AiEncyclopedia.Application.Admin.Commands.PublishArticle;
using AiEncyclopedia.Application.Admin.Queries.GetAdminArticles;
using AiEncyclopedia.Application.Admin.Queries.GetAdminUsers;
using AiEncyclopedia.Application.Admin.Queries.GetAIJobs;
using AiEncyclopedia.Application.Articles.Commands.GenerateArticleAI;
using AiEncyclopedia.Domain.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AiEncyclopedia.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminController(IMediator mediator) => _mediator = mediator;

    // Articles
    [HttpGet("articles")]
    public async Task<IActionResult> GetArticles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] ArticleStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminArticlesQuery(page, Math.Clamp(pageSize, 1, 100), status), ct);
        return Ok(result);
    }

    [HttpPost("articles/generate")]
    public async Task<IActionResult> GenerateArticle([FromBody] GenerateArticleRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateArticleAICommand(req.TopicId, req.Difficulty, req.Provider), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("articles/{id}/publish")]
    public async Task<IActionResult> PublishArticle(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishArticleCommand(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("articles/{id}")]
    public async Task<IActionResult> DeleteArticle(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteArticleCommand(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminUsersQuery(page, Math.Clamp(pageSize, 1, 100)), ct);
        return Ok(result);
    }

    // AI Jobs
    [HttpGet("ai-jobs")]
    public async Task<IActionResult> GetAIJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] AIJobStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAIJobsQuery(page, Math.Clamp(pageSize, 1, 100), status), ct);
        return Ok(result);
    }
}

public record GenerateArticleRequest(Guid TopicId, DifficultyLevel Difficulty, AIProviderName? Provider);
