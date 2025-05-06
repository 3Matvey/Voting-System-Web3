using System.Linq.Expressions;
using Voting.Domain.Aggregates;

namespace Voting.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторий для off-chain конфигурации сессии голосования.
    /// </summary>
    public interface IVotingSessionRepository : IRepository<VotingSessionAggregate>
    {
        Task<VotingSessionAggregate?> GetByIdAsync(uint id, 
            CancellationToken cancellationToken = default, 
            params Expression<Func<VotingSessionAggregate, object>>[] includesProperties);
    }
}
