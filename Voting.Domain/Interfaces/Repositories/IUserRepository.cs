using System.Linq.Expressions;
using Voting.Domain.Aggregates;

namespace Voting.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default,
            params Expression<Func<User, object>>[] includesProperties);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByBlockchainAddressAsync(string address);
    }
}
