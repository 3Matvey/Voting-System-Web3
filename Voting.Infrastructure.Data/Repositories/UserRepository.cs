using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Voting.Domain.Aggregates;
using Voting.Domain.Interfaces.Repositories;

namespace Voting.Infrastructure.Data.Repositories
{
    public class UserRepository(AppDbContext context)
        : EfRepository<User>(context), IUserRepository
    {
        public async Task<User?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default,
            params Expression<Func<User, object>>[] includesProperties)
        {
            var query = QueryWithIncludes(includesProperties);
            return await query.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByBlockchainAddressAsync(string address,
            CancellationToken cancellationToken = default,
            params Expression<Func<User, object>>[] includesProperties)
        {
            var query = QueryWithIncludes(includesProperties);
            return await query.FirstOrDefaultAsync(u => u.BlockchainAddress == address, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email,
            CancellationToken cancellationToken = default,
            params Expression<Func<User, object>>[] includesProperties)
        {
            var query = QueryWithIncludes(includesProperties);
            return await query.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public Task<string> GetBlockchainAddressAsync(Guid userId, CancellationToken ct = default)
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.BlockchainAddress)
                .SingleAsync(ct);
        }
    }
}
