using CvManagementSystem.Application.DTOs.Auth;
using CvManagementSystem.Application.Common;
namespace CvManagementSystem.Application.Interfaces;
public interface IAuthService
{
    Task<(AuthResponse? Data, Error? Error)> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken = default);
    Task<(AuthResponse? Data, Error? Error)> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default);
}
