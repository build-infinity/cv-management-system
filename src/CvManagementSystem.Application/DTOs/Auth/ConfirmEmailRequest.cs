using System.ComponentModel.DataAnnotations;

namespace CvManagementSystem.Application.DTOs.Auth;
public class ConfirmEmailRequest
{
    public Guid UserId { get; set; }
    [Required, StringLength(64, MinimumLength = 64)]
    public string Token { get; set; } = string.Empty;
}
