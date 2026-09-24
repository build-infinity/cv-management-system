using System.Security.Cryptography;
using System.Text;
using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Domain.Entities;
using CvManagementSystem.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace CvManagementSystem.Infrastructure.Email;
public class EmailVerificationService(ApplicationDbContext context, IOptions<EmailVerificationOptions> options)
    : IEmailVerificationService
{
    public void Queue(User user)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var now = DateTime.UtcNow;
        user.EmailVerificationTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        user.EmailVerificationExpiresAtUtc = now.AddHours(options.Value.ExpirationHours);
        user.EmailVerificationQueuedAtUtc = now;

        var link = $"{options.Value.PublicBaseUrl.TrimEnd('/')}/api/auth/confirm-email?userId={user.Id}&token={token}";
        context.EmailMessages.Add(new EmailMessage
        {
            Recipient = user.Email,
            Subject = "Confirm your email",
            Body = $"Confirm your email by opening this link:\n\n{link}\n\nThis link expires in {options.Value.ExpirationHours} hours. If you did not create this account, ignore this email.",
            CreatedAtUtc = now,
            NextAttemptAtUtc = now,
            ExpiresAtUtc = user.EmailVerificationExpiresAtUtc.Value
        });
    }

    public bool IsTokenValid(User user, string token)
    {
        if (user.EmailConfirmed || user.EmailVerificationTokenHash is null
            || user.EmailVerificationExpiresAtUtc is null
            || user.EmailVerificationExpiresAtUtc <= DateTime.UtcNow || token.Length != 64)
            return false;

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash), Encoding.UTF8.GetBytes(user.EmailVerificationTokenHash));
    }
}
