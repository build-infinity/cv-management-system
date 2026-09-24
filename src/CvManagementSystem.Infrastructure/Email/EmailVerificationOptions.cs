namespace CvManagementSystem.Infrastructure.Email;
public class EmailVerificationOptions
{
    public string PublicBaseUrl { get; set; } = "https://localhost:7186";
    public int ExpirationHours { get; set; } = 24;
}
