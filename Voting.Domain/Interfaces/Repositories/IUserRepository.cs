using System.Linq.Expressions;
using Voting.Domain.Aggregates;

namespace Voting.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default,
            params Expression<Func<User, object>>[] includesProperties);
        Task<User?> GetByEmailAsync(string email,
            CancellationToken cancellationToken = default,
            params Expression<Func<User, object>>[] includesProperties);
        Task<User?> GetByBlockchainAddressAsync(string address,
            CancellationToken cancellationToken = default,
            params Expression<Func<User, object>>[] includesProperties);

        Task<string> GetBlockchainAddressAsync(Guid userId, 
            CancellationToken ct = default);
    }
}
