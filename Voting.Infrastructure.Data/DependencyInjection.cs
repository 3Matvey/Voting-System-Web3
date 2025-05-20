using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voting.Domain.Interfaces.Repositories;
using Voting.Domain.Interfaces;
using Voting.Infrastructure.Data.Repositories; 

namespace Voting.Infrastructure.Data
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Регистрирует DbContext и все «infrastructure»-сервисы для работы с БД.
        /// Ожидается секция "ConnectionStrings" → "VotingDatabase" в конфиге.
        /// </summary>
        public static IServiceCollection AddDataServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("VotingDatabase"))
            );

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVotingSessionRepository, VotingSessionRepository>();

            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            return services;
        }
    }
}
