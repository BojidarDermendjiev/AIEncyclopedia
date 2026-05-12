using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<ApiResponse<User>>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ApiResponse<User>>
{
    private readonly IApplicationDbContext _db;

    public GetUserProfileQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<User>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        return user == null
            ? ApiResponse<User>.Fail("User not found.")
            : ApiResponse<User>.Ok(user);
    }
}
