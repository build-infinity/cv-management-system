using CvManagementSystem.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CvManagementSystem.Api.Controllers;
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ObjectResult HandleError(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return StatusCode(statusCode, new { message = error.Message });
    }
}
