// Voting.Application/Interfaces/IJwtTokenService.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Voting.Application.Interfaces
{
    /// <summary>
    /// Результат генерации токенов.
    /// </summary>
    public record JwtTokenResult(
        string Token,
        string RefreshToken,
        DateTime ExpiresAt);

    /// <summary>
    /// Отвечает за генерацию access- и refresh-токенов.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <param name="userId">Id пользователя (sub claim)</param>
        /// <param name="role">роль пользователя (role claim)</param>
        /// <param name="extraClaims">дополнительные claims (по желанию)</param>
        JwtTokenResult CreateTokens(
            Guid userId,
            string role,
            IEnumerable<Claim>? extraClaims = null
        );
    }
}
