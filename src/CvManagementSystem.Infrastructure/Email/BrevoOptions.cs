namespace CvManagementSystem.Infrastructure.Email;
public class BrevoOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@example.com";
    public string FromName { get; set; } = "CV Management System";
}
