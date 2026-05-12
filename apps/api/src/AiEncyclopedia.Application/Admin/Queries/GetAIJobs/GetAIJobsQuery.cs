using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Admin.Queries.GetAIJobs;

public record AIJobDto(Guid Id, AIJobType JobType, AIJobStatus Status, AIProviderName Provider, string? ErrorMessage, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt);

public record GetAIJobsQuery(int Page = 1, int PageSize = 50, AIJobStatus? Status = null) : IRequest<ApiResponse<PaginatedResult<AIJobDto>>>;

public class GetAIJobsQueryHandler : IRequestHandler<GetAIJobsQuery, ApiResponse<PaginatedResult<AIJobDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAIJobsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PaginatedResult<AIJobDto>>> Handle(GetAIJobsQuery request, CancellationToken ct)
    {
        var query = _db.AIJobs.AsQueryable();
        if (request.Status.HasValue)
            query = query.Where(j => j.Status == request.Status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new AIJobDto(j.Id, j.JobType, j.Status, j.Provider, j.ErrorMessage, j.CreatedAt, j.CompletedAt))
            .ToListAsync(ct);

        return ApiResponse<PaginatedResult<AIJobDto>>.Ok(new PaginatedResult<AIJobDto>(items, total, request.Page, request.PageSize));
    }
}
