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
            // 1. Загрузим трекнутую сессию вместе с кандидатами
            var tracked = await QueryWithIncludes(s => s.Candidates)
                .FirstOrDefaultAsync(s => s.Id == detached.Id, cancellationToken)
                ?? throw new InvalidOperationException($"VotingSession {detached.Id} not found");

            // 2. Синхронизируем скалярные свойства агрегата
            _context.Entry(tracked).CurrentValues.SetValues(detached);

            // 3. Синхронизируем коллекцию Candidates

            // 3.1. Удаляем кандидатов, которых больше нет в incoming
            var toRemove = tracked.Candidates
                .Where(c => detached.Candidates.All(dc => dc.Id != c.Id))
                .ToList();
            foreach (var candidate in toRemove)
                tracked.Candidates.Remove(candidate);

            // 3.2. Добавляем новых кандидатов из incoming
            var toAdd = detached.Candidates
                .Where(dc => tracked.Candidates.All(c => c.Id != dc.Id))
                .ToList();
            foreach (var candidate in toAdd)
                tracked.Candidates.Add(candidate);

            // 3.3. Обновляем изменившихся кандидатов
            foreach (var existing in tracked.Candidates)
            {
                var incoming = detached.Candidates
                    .FirstOrDefault(dc => dc.Id == existing.Id);
                if (incoming != null)
                    _context.Entry(existing).CurrentValues.SetValues(incoming);
            }

            // Сохранять будем снаружи (SaveChangesAsync)
        }

    }
}
