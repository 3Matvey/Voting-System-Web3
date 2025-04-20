using Voting.Application.Events;

namespace Voting.Application.Interfaces
{
    /// <summary>
    /// Слушатель событий смарт‑контракта. 
    /// </summary>
    public interface IContractEventListener : IAsyncDisposable
    {
        /// <summary>Запускает подписки на события.</summary>
        Task StartAsync();

        /// <summary>Останавливает подписки.</summary>
        Task StopAsync();

        event EventHandler<SessionCreatedEventArgs> SessionCreated;
        event EventHandler<CandidateAddedEventArgs> CandidateAdded;
        event EventHandler<CandidateRemovedEventArgs> CandidateRemoved;
        event EventHandler<VotingStartedEventArgs> VotingStarted;
        event EventHandler<VotingEndedEventArgs> VotingEnded;
        event EventHandler<VoteCastEventArgs> VoteCast;
    }
}
