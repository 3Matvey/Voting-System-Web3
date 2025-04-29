using Voting.Domain.Interfaces.Repositories;

namespace Voting.Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IUserRepository UserRepository { get; }
        IVotingSessionRepository VotingSessionRepository { get; }
        Task CommitAsync();
    }
}