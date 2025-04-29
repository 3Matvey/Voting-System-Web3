using System.Linq.Expressions;
using Voting.Domain.Common;

namespace Voting.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Общий CRUD-контракт для агрегатных корней.
    /// </summary>
    public interface IRepository<T, TKey>
        where T : AggregateRoot<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includesProperties);
        Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includesProperties);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
    }
}
