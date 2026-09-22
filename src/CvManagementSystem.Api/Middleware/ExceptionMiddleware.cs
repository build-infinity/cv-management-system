using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CvManagementSystem.Api.Middleware;
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted
            && !context.RequestAborted.IsCancellationRequested)
        {
            var isDuplicateEmail = exception is DbUpdateException
            {
                InnerException: PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "IX_Users_Email"
                }
            };

            if (!isDuplicateEmail)
                logger.LogError(exception, "An unhandled exception occurred while processing the request.");

            context.Response.Clear();
            context.Response.StatusCode = isDuplicateEmail
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                message = isDuplicateEmail
                    ? "This email is already registered."
                    : "An unexpected error occurred."
            }, context.RequestAborted);
        }
    }
}
