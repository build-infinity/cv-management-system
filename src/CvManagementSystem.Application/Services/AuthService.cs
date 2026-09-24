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
    IJwtTokenGenerator jwtTokenGenerator,
    IEmailVerificationService emailVerificationService) : IAuthService
{
    public async Task<(MessageResponse? Data, Error? Error)> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken = default)
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
        emailVerificationService.Queue(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (new MessageResponse("Account created. Check your email to confirm your address."), null);
    }

    public async Task<(AuthResponse? Data, Error? Error)> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return (null, new Error(ErrorType.Unauthorized, "Email or password is incorrect."));

        if (!user.EmailConfirmed)
            return (null, new Error(ErrorType.Forbidden, "Confirm your email before signing in."));

        return (new AuthResponse(jwtTokenGenerator.Generate(user)), null);
    }

    public async Task<(AuthResponse? Data, Error? Error)> GoogleSignInAsync(GoogleUserInfo googleUser, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGoogleIdAsync(googleUser.Subject, cancellationToken);
        if (user is not null)
            return (new AuthResponse(jwtTokenGenerator.Generate(user)), null);

        var email = googleUser.Email.Trim().ToLowerInvariant();
        if (await userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            return (null, new Error(ErrorType.Conflict, "This email is already registered. Sign in using your existing account."));

        if (email.Length > 254 || googleUser.Subject.Length > 255
            || googleUser.FirstName.Length > 100 || googleUser.LastName.Length > 100)
            return (null, new Error(ErrorType.Validation, "Google profile exceeds the supported field lengths."));

        user = new User
        {
            GoogleId = googleUser.Subject,
            EmailConfirmed = true,
            Email = email,
            FirstName = googleUser.FirstName,
            LastName = googleUser.LastName
        };

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return (new AuthResponse(jwtTokenGenerator.Generate(user)), null);
    }

    public async Task<(MessageResponse? Data, Error? Error)> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !emailVerificationService.IsTokenValid(user, request.Token))
            return (null, new Error(ErrorType.Validation, "The confirmation link is invalid or expired."));

        user.EmailConfirmed = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationExpiresAtUtc = null;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return (new MessageResponse("Email confirmed. You can now sign in."), null);
    }

    public async Task<(MessageResponse? Data, Error? Error)> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is not null && !user.EmailConfirmed
            && (user.EmailVerificationQueuedAtUtc is null || user.EmailVerificationQueuedAtUtc <= DateTime.UtcNow.AddMinutes(-1)))
        {
            emailVerificationService.Queue(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return (new MessageResponse("If the account needs confirmation, an email will be sent."), null);
    }
}
