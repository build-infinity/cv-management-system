using CvManagementSystem.Application.DTOs.Auth;
using CvManagementSystem.Application.Common;
namespace CvManagementSystem.Application.Interfaces;
public interface IAuthService
{
    Task<(MessageResponse? Data, Error? Error)> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken = default);
    Task<(AuthResponse? Data, Error? Error)> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default);
    Task<(AuthResponse? Data, Error? Error)> GoogleSignInAsync(GoogleUserInfo googleUser, CancellationToken cancellationToken = default);
    Task<(MessageResponse? Data, Error? Error)> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task<(MessageResponse? Data, Error? Error)> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default);
}
