using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Voting.Domain.Common;
using Voting.Domain.Interfaces.Repositories;

namespace Voting.Infrastructure.Data.Repositories
{
    public class EfRepository<T>(AppDbContext context) : IRepository<T>
        where T : AggregateRoot
    {
        protected readonly AppDbContext _context = context;
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        protected IQueryable<T> QueryWithIncludes(params Expression<Func<T, object>>[] includesProperties)
        {
            IQueryable<T> query = _dbSet.AsQueryable();
            foreach (var prop in includesProperties)
            {
                query = query.Include(prop);
            }
            return query;
        }

        public virtual async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default)
            => await _dbSet.ToListAsync(cancellationToken);

        public virtual async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includesProperties)
        {
            var query = QueryWithIncludes(includesProperties);

            return await query.Where(filter).ToListAsync(cancellationToken);
        }

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(filter, cancellationToken);
        }

        
    }
}
