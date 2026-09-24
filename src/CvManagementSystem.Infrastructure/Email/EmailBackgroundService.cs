using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CvManagementSystem.Infrastructure.Email;
public class EmailBackgroundService(IServiceScopeFactory scopeFactory, ILogger<EmailBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await SendNextAsync(stoppingToken))
                    continue;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Email queue processing failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task<bool> SendNextAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;

        // Lock one message so multiple application instances do not send it simultaneously.
        var messages = await context.EmailMessages.FromSqlInterpolated($"""
            SELECT * FROM "EmailMessages"
            WHERE "SentAtUtc" IS NULL AND "NextAttemptAtUtc" <= {now} AND "ExpiresAtUtc" > {now}
            ORDER BY "NextAttemptAtUtc", "CreatedAtUtc"
            LIMIT 1 FOR UPDATE SKIP LOCKED
            """).ToListAsync(cancellationToken);
        var message = messages.SingleOrDefault();
        if (message is null)
            return false;

        message.Attempts++;
        try
        {
            await sender.SendAsync(message.Recipient, message.Subject, message.Body, cancellationToken);
            message.SentAtUtc = DateTime.UtcNow;
            message.Body = string.Empty;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            message.NextAttemptAtUtc = DateTime.UtcNow.AddMinutes(Math.Min(60, Math.Pow(2, Math.Min(message.Attempts, 6))));
            logger.LogWarning("Email {EmailId} attempt {Attempt} failed ({ErrorType}); it will be retried.",
                message.Id, message.Attempts, exception.GetType().Name);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
