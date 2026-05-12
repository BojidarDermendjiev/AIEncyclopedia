using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Admin.Queries.GetAdminUsers;

public record AdminUserDto(Guid Id, string Email, string DisplayName, SubscriptionTier Tier, bool IsAdmin, bool IsActive, DateTimeOffset CreatedAt);

public record GetAdminUsersQuery(int Page = 1, int PageSize = 50) : IRequest<ApiResponse<PaginatedResult<AdminUserDto>>>;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, ApiResponse<PaginatedResult<AdminUserDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAdminUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<PaginatedResult<AdminUserDto>>> Handle(GetAdminUsersQuery request, CancellationToken ct)
    {
        var total = await _db.Users.CountAsync(ct);
        var items = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new AdminUserDto(u.Id, u.Email, u.DisplayName, u.SubscriptionTier, u.IsAdmin, u.IsActive, u.CreatedAt))
            .ToListAsync(ct);

        return ApiResponse<PaginatedResult<AdminUserDto>>.Ok(new PaginatedResult<AdminUserDto>(items, total, request.Page, request.PageSize));
    }
}
