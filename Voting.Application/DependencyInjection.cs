using Microsoft.Extensions.DependencyInjection;
using Voting.Application.Projections;

namespace Voting.Application
{
    /// <summary>
    /// Регистрирует все «application»‑сервисы: проекции, use‑case’ы и т.п.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сборку Voting.Application в DI‑контейнер.
        /// </summary>
        public static IServiceCollection AddVotingApplication(this IServiceCollection services)
        {
            services.AddHostedService<VotingSessionProjection>();

            return services;
        }
    }
}
