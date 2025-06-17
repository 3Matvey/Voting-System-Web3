using Microsoft.AspNetCore.Mvc;
using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Application.UseCases.Commands.Auth;
using Voting.Application.UseCases.Commands.User;
using Voting.Domain.Entities.ValueObjects;
using Voting.Shared.ResultPattern;

namespace Voting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(
        RegisterUseCase registerUseCase,
        LoginUseCase loginUseCase,
        VerifyEmailUseCase verifyEmail,
        VerifyPassportUseCase verifyPassport
    ) : ControllerBaseWithResult
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

        [HttpPost("{userId}/verify-email")]
        //[Authorize]
        public async Task<IActionResult> VerifyEmail([FromRoute] Guid userId)
        {
            var result = await verifyEmail.Execute(userId);
            return result.Match<IActionResult>(
                onSuccess: () => Ok(new { message = "Email успешно верифицирован." }),
                onFailure: err => err.Code switch
                {
                    "User.NotFound" => NotFound(new { err.Code, err.Description }),
                    "InvalidRequest" => BadRequest(new { err.Code, err.Description }),
                    _ => StatusCode(500, new { err.Code, err.Description })
                }
            );
        }

        /// <summary>
        /// Верификация паспорта (пользователь должен быть авторизован).
        /// </summary>
        [HttpPost("{userId}/verify-passport")]
       // [Authorize]
        public async Task<IActionResult> VerifyPassport(
            [FromRoute] Guid userId,
            [FromBody] PassportIdentifierDto passportDto)
        {
            var passport = new PassportIdentifier(passportDto.Series + passportDto.Number); //FIXME 
                          
            var result = await verifyPassport.Execute(userId, passport);
            return result.Match<IActionResult>(
                onSuccess: () => Ok(new { message = "Паспорт успешно верифицирован." }),
                onFailure: err => err.Code switch
                {
                    "User.NotFound" => NotFound(new { err.Code, err.Description }),
                    "InvalidRequest" => BadRequest(new { err.Code, err.Description }),
                    _ => StatusCode(500, new { err.Code, err.Description })
                }
            );
        }
    }
}
