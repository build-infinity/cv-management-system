using CvManagementSystem.Application.Abstractions;
using CvManagementSystem.Application.Common;
using CvManagementSystem.Application.DTOs.Auth;
using CvManagementSystem.Application.Interfaces;
using CvManagementSystem.Domain.Entities;

namespace CvManagementSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailVerificationService _emailVerificationService;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailVerificationService emailVerificationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailVerificationService = emailVerificationService;
    }

    public async Task<(MessageResponse? Data, Error? Error)> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        if (await _userRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return (null, new Error(ErrorType.Conflict, "This email is already registered."));
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _userRepository.Add(user);
        _emailVerificationService.Queue(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (new MessageResponse("Account created. Check your email to confirm your address."), null);
    }

    public async Task<(AuthResponse? Data, Error? Error)> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return (null, new Error(ErrorType.Unauthorized, "Email or password is incorrect."));
        }

        if (!user.EmailConfirmed)
        {
            return (null, new Error(ErrorType.Forbidden, "Confirm your email before signing in."));
        }

        return (new AuthResponse(_jwtTokenGenerator.Generate(user)), null);
    }

    public async Task<(AuthResponse? Data, Error? Error)> GoogleSignInAsync(GoogleUserInfo googleUser, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByGoogleIdAsync(googleUser.Subject, cancellationToken);
        if (user is not null)
        {
            return (new AuthResponse(_jwtTokenGenerator.Generate(user)), null);
        }

        var email = NormalizeEmail(googleUser.Email);
        if (await _userRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return (null, new Error(ErrorType.Conflict, "This email is already registered. Sign in using your existing account."));
        }

        if (email.Length > 254 || googleUser.Subject.Length > 255
            || googleUser.FirstName.Length > 100 || googleUser.LastName.Length > 100)
        {
            return (null, new Error(ErrorType.Validation, "Google profile exceeds the supported field lengths."));
        }

        user = new User
        {
            GoogleId = googleUser.Subject,
            EmailConfirmed = true,
            Email = email,
            FirstName = googleUser.FirstName,
            LastName = googleUser.LastName
        };

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (new AuthResponse(_jwtTokenGenerator.Generate(user)), null);
    }

    public async Task<(MessageResponse? Data, Error? Error)> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !_emailVerificationService.IsTokenValid(user, request.Token))
        {
            return (null, new Error(ErrorType.Validation, "The confirmation link is invalid or expired."));
        }

        user.EmailConfirmed = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationExpiresAtUtc = null;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (new MessageResponse("Email confirmed. You can now sign in."), null);
    }

    public async Task<(MessageResponse? Data, Error? Error)> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is not null && !user.EmailConfirmed
            && (user.EmailVerificationQueuedAtUtc is null || user.EmailVerificationQueuedAtUtc <= DateTime.UtcNow.AddMinutes(-1)))
        {
            _emailVerificationService.Queue(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return (new MessageResponse("If the account needs confirmation, an email will be sent."), null);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
