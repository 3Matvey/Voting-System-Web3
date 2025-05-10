using Voting.Domain.Interfaces.Repositories;

namespace Voting.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IVotingSessionRepository VotingSessions { get; }
        Task CommitAsync();
    }
}