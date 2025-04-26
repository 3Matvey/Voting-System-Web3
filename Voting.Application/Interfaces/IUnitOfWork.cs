namespace Voting.Application.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IVotingSessionRepository VotingSessionRepository { get; }
        Task CommitAsync();
    }
}