using System.ComponentModel.DataAnnotations;

namespace CvManagementSystem.Application.DTOs.Auth;
public class SignInRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;
    [Required, StringLength(72)]
    public string Password { get; set; } = string.Empty;
}
