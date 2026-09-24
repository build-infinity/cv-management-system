using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CvManagementSystem.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted
            && !context.RequestAborted.IsCancellationRequested)
        {
            var isAccountConflict = exception is DbUpdateException
            {
                InnerException: PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "IX_Users_Email" or "IX_Users_GoogleId"
                }
            };

            if (!isAccountConflict)
            {
                _logger.LogError(exception, "An unhandled exception occurred while processing the request.");
            }

            context.Response.Clear();
            context.Response.StatusCode = isAccountConflict
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                message = isAccountConflict
                    ? "This email or Google account is already registered."
                    : "An unexpected error occurred."
            }, context.RequestAborted);
        }
    }
}
