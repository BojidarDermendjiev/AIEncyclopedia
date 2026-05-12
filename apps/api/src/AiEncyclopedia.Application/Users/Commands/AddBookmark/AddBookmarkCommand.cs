using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Application.Common.Models;
using AiEncyclopedia.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiEncyclopedia.Application.Users.Commands.AddBookmark;

public record AddBookmarkCommand(Guid UserId, Guid ArticleId) : IRequest<ApiResponse<bool>>;

public class AddBookmarkCommandValidator : AbstractValidator<AddBookmarkCommand>
{
    public AddBookmarkCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ArticleId).NotEmpty();
    }
}

public class AddBookmarkCommandHandler : IRequestHandler<AddBookmarkCommand, ApiResponse<bool>>
{
    private readonly IApplicationDbContext _db;

    public AddBookmarkCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<bool>> Handle(AddBookmarkCommand request, CancellationToken ct)
    {
        var exists = await _db.UserBookmarks
            .AnyAsync(b => b.UserId == request.UserId && b.ArticleId == request.ArticleId, ct);

        if (exists)
            return ApiResponse<bool>.Ok(true, "Already bookmarked.");

        var articleExists = await _db.Articles.AnyAsync(a => a.Id == request.ArticleId, ct);
        if (!articleExists)
            return ApiResponse<bool>.Fail("Article not found.");

        _db.UserBookmarks.Add(new UserBookmark { UserId = request.UserId, ArticleId = request.ArticleId });
        await _db.SaveChangesAsync(ct);
        return ApiResponse<bool>.Ok(true);
    }
}
