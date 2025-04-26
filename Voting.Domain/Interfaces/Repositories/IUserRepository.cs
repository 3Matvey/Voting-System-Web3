using Voting.Domain.Entities;

namespace Voting.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByBlockchainAddressAsync(string address);
        Task AddAsync(User user);
    }
}
