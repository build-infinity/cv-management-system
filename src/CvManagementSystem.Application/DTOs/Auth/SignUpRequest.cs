using System.ComponentModel.DataAnnotations;

namespace CvManagementSystem.Application.DTOs.Auth;
public class SignUpRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;
    [Required, StringLength(72, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
