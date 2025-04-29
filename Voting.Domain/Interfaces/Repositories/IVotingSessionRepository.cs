using Voting.Domain.Aggregates;
using Voting.Domain.Entities.ValueObjects;

namespace Voting.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторий для off-chain конфигурации сессии голосования.
    /// </summary>
    public interface IVotingSessionRepository : IRepository<VotingSessionAggregate, uint>;
}
