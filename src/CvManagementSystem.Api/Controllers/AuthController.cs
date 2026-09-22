using System.Text;
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
    public async Task<ActionResult<AuthResponse>> SignUp(SignUpRequest request, CancellationToken cancellationToken)
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
}
