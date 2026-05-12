using AiEncyclopedia.Application.Users.Commands.AddBookmark;
using AiEncyclopedia.Application.Users.Commands.RecordReadingHistory;
using AiEncyclopedia.Application.Users.Commands.RemoveBookmark;
using AiEncyclopedia.Application.Users.Queries.GetBookmarks;
using AiEncyclopedia.Application.Users.Queries.GetReadingHistory;
using AiEncyclopedia.Application.Users.Queries.GetUserProfile;
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
[Authorize]
[EnableRateLimiting("api")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = GetUserIdOrFail();
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new GetUserProfileQuery(userId.Value), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarks(CancellationToken ct)
    {
        var userId = GetUserIdOrFail();
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new GetBookmarksQuery(userId.Value), ct);
        return Ok(result);
    }

    [HttpPost("bookmarks/{articleId}")]
    public async Task<IActionResult> AddBookmark(Guid articleId, CancellationToken ct)
    {
        var userId = GetUserIdOrFail();
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new AddBookmarkCommand(userId.Value, articleId), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("bookmarks/{articleId}")]
    public async Task<IActionResult> RemoveBookmark(Guid articleId, CancellationToken ct)
    {
        var userId = GetUserIdOrFail();
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new RemoveBookmarkCommand(userId.Value, articleId), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var userId = GetUserIdOrFail();
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new GetReadingHistoryQuery(userId.Value, Math.Clamp(limit, 1, 50)), ct);
        return Ok(result);
    }

    [HttpPost("history")]
    public async Task<IActionResult> RecordHistory([FromBody] RecordHistoryRequest req, CancellationToken ct)
    {
        var userId = GetUserIdOrFail();
        if (userId == null) return Unauthorized();
        var result = await _mediator.Send(new RecordReadingHistoryCommand(userId.Value, req.ArticleId, req.ProgressPercent), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetUserIdOrFail()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idStr, out var id) ? id : null;
    }
}

public record RecordHistoryRequest(Guid ArticleId, int ProgressPercent);
