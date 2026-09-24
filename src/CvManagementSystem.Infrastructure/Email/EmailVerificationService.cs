using System.Security.Cryptography;
using System.Text;
using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Domain.Entities;
using CvManagementSystem.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace CvManagementSystem.Infrastructure.Email;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IOptions<EmailVerificationOptions> _options;

    public EmailVerificationService(ApplicationDbContext context, IOptions<EmailVerificationOptions> options)
    {
        _context = context;
        _options = options;
    }

    public void Queue(User user)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var now = DateTime.UtcNow;
        user.EmailVerificationTokenHash = HashToken(token);
        user.EmailVerificationExpiresAtUtc = now.AddHours(_options.Value.ExpirationHours);
        user.EmailVerificationQueuedAtUtc = now;

        var link = $"{_options.Value.PublicBaseUrl.TrimEnd('/')}/api/auth/confirm-email?userId={user.Id}&token={token}";
        _context.EmailMessages.Add(new EmailMessage
        {
            Recipient = user.Email,
            Subject = "Confirm your email",
            Body = $"Confirm your email by opening this link:\n\n{link}\n\nThis link expires in {_options.Value.ExpirationHours} hours. If you did not create this account, ignore this email.",
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
        {
            return false;
        }

        var hash = HashToken(token);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash), Encoding.UTF8.GetBytes(user.EmailVerificationTokenHash));
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
