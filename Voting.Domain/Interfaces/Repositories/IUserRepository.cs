using Voting.Domain.Aggregates;

namespace Voting.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByBlockchainAddressAsync(string address);
    }
}
