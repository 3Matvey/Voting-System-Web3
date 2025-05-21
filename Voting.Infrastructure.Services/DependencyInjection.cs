using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voting.Application.Interfaces;
using Voting.Domain.Interfaces;

namespace Voting.Infrastructure.Services
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; }
    }

    public static class DependencyInjection
    {
        public static IServiceCollection AddDataServices(
               this IServiceCollection services,
               IConfiguration configuration)
        {
            services.Configure<JwtSettings>(
              configuration.GetSection("Jwt"));

            services.AddSingleton<IDomainEventPublisher, InMemoryDomainEventPublisher>();

            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();

            return services;
        }
    }
}
