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

            // 1) Сохраняем скаляры FIXME разобраться как все работает
            _context.Entry(tracked).CurrentValues.SetValues(detached);

            // 2) Удаляем “лишних” кандидатов
            var toRemove = tracked.Candidates
                .Where(c => detached.Candidates.All(dc => dc.Id != c.Id))
                .ToList();
            foreach (var old in toRemove)
                _context.Entry(old).State = EntityState.Deleted;

            // Нужен “промежуточный” SaveChanges, чтобы DELETE отработал в базе ДО INSERT’ов
            await _context.SaveChangesAsync(cancellationToken);

            // 3) Теперь обновляем и добавляем
            foreach (var incoming in detached.Candidates)
            {
                var existing = tracked.Candidates.FirstOrDefault(c => c.Id == incoming.Id);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(incoming);
                    // (по умолчанию уже Modified)
                }
                else
                {
                    tracked.Candidates.Add(incoming);
                    // (по умолчанию Added)
                }
            }

            // 4) Финальный Save
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
