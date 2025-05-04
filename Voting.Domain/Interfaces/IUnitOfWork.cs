using Voting.Domain.Interfaces.Repositories;

namespace Voting.Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IUserRepository Users { get; }
        IVotingSessionRepository VotingSessions { get; }
        Task CommitAsync();
    }
}