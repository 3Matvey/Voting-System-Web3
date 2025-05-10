using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Voting.Domain.Aggregates;
using Voting.Domain.Interfaces.Repositories;

namespace Voting.Infrastructure.Data.Repositories
{
    public class VotingSessionRepository(AppDbContext context)
        : EfRepository<VotingSessionAggregate>(context), IVotingSessionRepository
    {
        public async Task<VotingSessionAggregate?> GetByIdAsync(uint id,
            CancellationToken cancellationToken = default,
            params Expression<Func<VotingSessionAggregate, object>>[] includesProperties)
        {
            var query = QueryWithIncludes(includesProperties);
            return await query.FirstOrDefaultAsync(vs => vs.Id == id, cancellationToken);
        }
    }
}
