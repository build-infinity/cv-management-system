using CvManagementSystem.Application.DTOs.Auth;
using CvManagementSystem.Application.Common;
using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Application.Interfaces;
using CvManagementSystem.Domain.Entities;

namespace CvManagementSystem.Application.Services;
public class AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<(AuthResponse? Data, Error? Error)> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            return (null, new Error(ErrorType.Conflict, "This email is already registered."));

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (new AuthResponse(jwtTokenGenerator.Generate(user)), null);
    }

    public async Task<(AuthResponse? Data, Error? Error)> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return (null, new Error(ErrorType.Unauthorized, "Email or password is incorrect."));

        return (new AuthResponse(jwtTokenGenerator.Generate(user)), null);
    }
}
