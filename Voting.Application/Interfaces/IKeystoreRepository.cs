using Voting.Domain.Entities;

namespace Voting.Application.Interfaces
{
    public interface IKeystoreRepository
    {
        Task<Keystore?> FindAsync(Guid userId, uint sessionId, CancellationToken ct = default);
        Task AddAsync(Keystore keystore, CancellationToken ct = default);
    }
}
