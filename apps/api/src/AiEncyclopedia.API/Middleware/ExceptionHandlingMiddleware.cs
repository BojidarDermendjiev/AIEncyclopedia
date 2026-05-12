using FluentValidation;
using System.Net;
using System.Text.Json;

namespace AiEncyclopedia.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ctx.Response.ContentType = "application/json";
            var errors = ex.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message = "Validation failed.",
                errors
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message = "An unexpected error occurred.",
                traceId = ctx.TraceIdentifier
            }));
        }
    }
}
