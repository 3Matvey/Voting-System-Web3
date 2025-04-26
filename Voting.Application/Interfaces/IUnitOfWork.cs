using Voting.Domain.Interfaces.Repositories;

namespace Voting.Application.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IVotingSessionRepository VotingSessionRepository { get; }
        Task CommitAsync();
    }
}