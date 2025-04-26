// Voting.Application.Projections/VotingSessionProjection.cs
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Voting.Application.Interfaces;
using Voting.Application.Events;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;

namespace Voting.Application.Projections
{
    /// <summary>
    /// HostedService, слушает события контракта и обновляет in-memory агрегаты.
    /// </summary>
    public class VotingSessionProjection(IContractEventListener listener, IVotingSessionRepository sessionRepo)
        : IHostedService
    {
        private readonly ConcurrentDictionary<uint, VotingSessionAggregate> _sessions
            = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            listener.SessionCreated += OnSessionCreated;
            listener.CandidateAdded += OnCandidateAdded;
            listener.CandidateRemoved += OnCandidateRemoved;
            listener.VotingStarted += OnVotingStarted;
            listener.VotingEnded += OnVotingEnded;
            listener.VoteCast += OnVoteCast;
            return listener.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await listener.StopAsync();
            listener.SessionCreated -= OnSessionCreated;
            listener.CandidateAdded -= OnCandidateAdded;
            listener.CandidateRemoved -= OnCandidateRemoved;
            listener.VotingStarted -= OnVotingStarted;
            listener.VotingEnded -= OnVotingEnded;
            listener.VoteCast -= OnVoteCast;
        }

        private VotingSessionAggregate GetOrCreate(uint sessionId)
            => _sessions.GetOrAdd(sessionId, _ => new VotingSessionAggregate());

        private void OnSessionCreated(object? _, SessionCreatedEventArgs e)
        {
            // достаём mode, который мы уже записали в репозиторий
            var mode = sessionRepo
                .GetRegistrationModeAsync(e.SessionId)
                .GetAwaiter().GetResult()    // в проекциях можно синхронно
                ?? throw new InvalidOperationException(
                     $"Mode для sessionId={e.SessionId} не найден");

            var domEvt = new SessionCreatedDomainEvent(
                e.SessionId,
                e.SessionAdmin,
                mode
            );

            GetOrCreate(e.SessionId).Apply(domEvt);
        }
        private void OnCandidateAdded(object? s, CandidateAddedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new CandidateAddedDomainEvent(e.SessionId, e.CandidateId, e.Name));

        private void OnCandidateRemoved(object? s, CandidateRemovedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new CandidateRemovedDomainEvent(e.SessionId, e.CandidateId));

        private void OnVotingStarted(object? s, VotingStartedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new VotingStartedDomainEvent(e.SessionId, e.StartTimeUtc, e.EndTimeUtc));

        private void OnVotingEnded(object? s, VotingEndedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new VotingEndedDomainEvent(e.SessionId, e.EndTimeUtc));

        private void OnVoteCast(object? s, VoteCastEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new VoteCastDomainEvent(e.SessionId, e.Voter, e.CandidateId));

        /// <summary>
        /// Доступ к агрегату из других компонентов.
        /// </summary>
        public VotingSessionAggregate? GetAggregate(uint sessionId)
            => _sessions.TryGetValue(sessionId, out var agg) ? agg : null;
    }
}
