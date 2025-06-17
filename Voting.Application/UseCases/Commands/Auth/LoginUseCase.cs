// Voting.Application/UseCases/Commands/Auth/LoginUseCase.cs
using System.ComponentModel.DataAnnotations;
using Voting.Application.DTOs.Requests;
using Voting.Application.DTOs.Responses;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Application.UseCases.Commands.Auth
{
    public class LoginUseCase(
        IUnitOfWork uow,
        IJwtTokenService jwt,
        IPasswordHasher hasher)
    {
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly IJwtTokenService _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        private readonly IPasswordHasher _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));

        public async Task<Result<AuthResponse>> Execute(LoginRequest request)
        {
            if (request is null)
                return Error.Validation("InvalidRequest", "Request must not be null.");

            if (!new EmailAddressAttribute().IsValid(request.Email))
                return Error.Validation("InvalidEmail", "Email is not a valid address.");

            var user = await _uow.Users.GetByEmailAsync(request.Email);
            if (user is null)
                return Error.NotFound("UserNotFound", $"No user with email {request.Email}.");

            if (!_hasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
                return Error.Validation("InvalidCredentials", "Email or password is incorrect.");

            var tokens = _jwt.CreateTokens(user.Id, user.Role.ToString());

            return Result<AuthResponse>.Success(new AuthResponse
            {
                UserId = user.Id,
                Token = tokens.Token,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = tokens.ExpiresAt
            });
        }
    }
}
