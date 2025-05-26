using Microsoft.Extensions.DependencyInjection;
using Voting.Application.Mappings;
using Voting.Application.Projections;
using Voting.Application.UseCases.Commands.Auth;
using Voting.Application.UseCases.Commands.VotingSession;
using Voting.Application.UseCases.Queries.VotingSession;

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
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            // Background projections
            services.AddHostedService<VotingSessionProjection>();

            // Auth use cases
            services.AddScoped<RegisterUseCase>();
            services.AddScoped<LoginUseCase>();

            // VotingSession commands
            services.AddScoped<AddCandidateUseCase>();
            services.AddScoped<CreateVotingSessionUseCase>();
            services.AddScoped<EndVotingUseCase>();
            services.AddScoped<StartVotingUseCase>();
            services.AddScoped<CastVoteUseCase>();

            // VotingSession queries
            services.AddScoped<GetVotingSessionUseCase>();
            services.AddScoped<GetVotingResultUseCase>();

            return services;
        }
    }
}
