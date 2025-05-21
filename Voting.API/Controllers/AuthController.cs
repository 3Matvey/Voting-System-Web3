// Voting.Api/Controllers/AuthController.cs
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Application.UseCases.Commands.Auth;
using Voting.Shared.ResultPattern;

namespace Voting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        RegisterUseCase registerUseCase,
        LoginUseCase loginUseCase) : ControllerBaseWithResult
    {

        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var result = await registerUseCase.Execute(request, cancellationToken);

            return result.Match<IActionResult, AuthResponse>(
                onSuccess: Ok,
                onFailure: err => err.Code == "EmailExists"
                    ? Conflict(new { err.Code, err.Description })
                    : BadRequest(new { err.Code, err.Description })
            );
        }

        /// <summary>
        /// Аутентификация пользователя.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result = await loginUseCase.Execute(request);

            return result.Match<IActionResult, AuthResponse>(
                onSuccess: Ok,
                onFailure: err => err.Code == "UserNotFound" || err.Code == "InvalidCredentials"
                    ? Unauthorized(new { err.Code, err.Description })
                    : BadRequest(new { err.Code, err.Description })
            );
        }
    }
}
