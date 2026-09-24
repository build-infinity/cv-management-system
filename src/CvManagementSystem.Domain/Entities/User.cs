namespace CvManagementSystem.Domain.Entities;
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationExpiresAtUtc { get; set; }
    public DateTime? EmailVerificationQueuedAtUtc { get; set; }
}
