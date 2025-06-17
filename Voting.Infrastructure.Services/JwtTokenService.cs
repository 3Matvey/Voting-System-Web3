// Voting.Infrastructure/Services/JwtTokenService.cs
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Voting.Application.Interfaces;
namespace Voting.Infrastructure.Services
{
    public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
    {
        private readonly JwtSettings _settings = options.Value;

        public JwtTokenResult CreateTokens(
            Guid userId,
            string role,
            IEnumerable<Claim>? extraClaims = null
        )
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_settings.ExpiryMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            if (extraClaims is not null)
                claims.AddRange(extraClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refreshToken = GenerateRefreshToken();
            return new JwtTokenResult(token, refreshToken, expires);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}
