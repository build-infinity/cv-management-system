using System.Text;
using System.Security.Claims;
using CvManagementSystem.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using CvManagementSystem.Application.Common;
using CvManagementSystem.Application.Interfaces;
using CvManagementSystem.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvManagementSystem.Api.Controllers;

[AllowAnonymous]
[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost("signup")]
    public async Task<ActionResult<MessageResponse>> SignUp(SignUpRequest request, CancellationToken cancellationToken)
    {
        if (Encoding.UTF8.GetByteCount(request.Password) > 72)
            return HandleError(new Error(ErrorType.Validation, "Password must not exceed 72 UTF-8 bytes."));

        var (data, error) = await authService.SignUpAsync(request, cancellationToken);
        if (error is not null)
            return HandleError(error);

        return StatusCode(StatusCodes.Status201Created, data);
    }

    [HttpPost("signin")]
    public async Task<ActionResult<AuthResponse>> SignIn(SignInRequest request, CancellationToken cancellationToken)
    {
        if (Encoding.UTF8.GetByteCount(request.Password) > 72)
            return HandleError(new Error(ErrorType.Validation, "Password must not exceed 72 UTF-8 bytes."));

        var (data, error) = await authService.SignInAsync(request, cancellationToken);
        if (error is not null)
            return HandleError(error);

        return Ok(data);
    }

    [HttpGet("confirm-email")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<MessageResponse>> ConfirmEmail([FromQuery] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await authService.ConfirmEmailAsync(request, cancellationToken);
        if (error is not null)
            return HandleError(error);
        return Ok(data);
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult<MessageResponse>> ResendVerification(ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        var (data, error) = await authService.ResendVerificationAsync(request, cancellationToken);
        if (error is not null)
            return HandleError(error);
        return Ok(data);
    }

    [HttpGet("google")]
    public async Task<IActionResult> GoogleSignIn([FromQuery] bool returnToLogin = false)
    {
        await HttpContext.SignOutAsync(AuthCookies.ExternalScheme);
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnToLogin ? "/login.html?google=callback" : Url.Action(nameof(GoogleCallback)),
            IsPersistent = false
        }, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(CancellationToken cancellationToken)
    {
        var result = await HttpContext.AuthenticateAsync(AuthCookies.ExternalScheme);
        await HttpContext.SignOutAsync(AuthCookies.ExternalScheme);

        if (!result.Succeeded || result.Principal is null)
            return HandleError(new Error(ErrorType.Unauthorized, "Google authentication is required."));

        var principal = result.Principal;
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var emailVerified = principal.FindFirstValue(AuthCookies.GoogleEmailVerifiedClaim);

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email)
            || !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
            return HandleError(new Error(ErrorType.Unauthorized, "A verified Google email is required."));

        var googleUser = new GoogleUserInfo(subject, email,
            principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty);

        var (data, error) = await authService.GoogleSignInAsync(googleUser, cancellationToken);
        if (error is not null)
            return HandleError(error);

        return Ok(data);
    }
}
