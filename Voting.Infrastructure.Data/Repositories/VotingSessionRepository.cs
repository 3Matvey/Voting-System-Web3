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

        public override async Task UpdateAsync(
            VotingSessionAggregate detached,
            CancellationToken cancellationToken = default)
        {
            var tracked = await QueryWithIncludes(s => s.Candidates)
                .FirstOrDefaultAsync(s => s.Id == detached.Id, cancellationToken)
                ?? throw new InvalidOperationException($"VotingSession {detached.Id} not found");

            // 1. Синхронизируем скалярные свойства агрегата
            _context.Entry(tracked).CurrentValues.SetValues(detached);

            // 2. Синхронизируем коллекцию кандидатов
            // 2.1. Удаляем кандидатов, которых больше нет в incoming
            var toRemove = tracked.Candidates
                .Where(c => detached.Candidates.All(dc => dc.Id != c.Id))
                .ToList();

            foreach (var candidate in toRemove)
                tracked.Candidates.Remove(candidate);

            // 2.2. Обновляем или добавляем новых кандидатов
            foreach (var incoming in detached.Candidates)
            {
                var existing = tracked.Candidates
                    .FirstOrDefault(c => c.Id == incoming.Id);

                if (existing != null)
                {
                    // Обновляем сущность
                    _context.Entry(existing).CurrentValues.SetValues(incoming);
                }
                else
                {
                    // Добавляем нового кандидата
                    tracked.Candidates.Add(incoming);
                }
            }
        }
    }
}
