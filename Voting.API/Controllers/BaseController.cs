using Voting.Shared.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace Voting.API.Controllers
{
    public abstract class ControllerBaseWithResult : ControllerBase
    {
        protected IActionResult Problem(Error error)
        {
            var statusCode = error.ErrorType switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.AccessUnauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.AccessForbidden => StatusCodes.Status403Forbidden,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };

            return base.Problem(
                statusCode: statusCode,
                title: error.Code,
                detail: error.Description
            );
        } 
    }
}
