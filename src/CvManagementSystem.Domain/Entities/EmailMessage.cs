namespace CvManagementSystem.Domain.Entities;
public class EmailMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime NextAttemptAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public int Attempts { get; set; }
}
