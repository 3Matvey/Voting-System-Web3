using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;
using Voting.Domain.Exceptions;
using Voting.Domain.Interfaces.Repositories;
using Voting.Application.Events;
using Voting.Application.Interfaces;

namespace Voting.Application.Projections
{
    /// <summary>
    /// HostedService, слушает on-chain события и обновляет in-memory агрегаты.
    /// </summary>
    public class VotingSessionProjection : IHostedService
    {
        private readonly IContractEventListener _listener;
        private readonly IVotingSessionRepository _sessionRepo;
        private readonly IUserRepository _userRepo;
        private readonly ConcurrentDictionary<uint, VotingSessionAggregate> _sessions
            = new();

        public VotingSessionProjection(
            IContractEventListener listener,
            IVotingSessionRepository sessionRepo,
            IUserRepository userRepo)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _sessionRepo = sessionRepo ?? throw new ArgumentNullException(nameof(sessionRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _listener.SessionCreated += OnSessionCreated;
            _listener.CandidateAdded += OnCandidateAdded;
            _listener.CandidateRemoved += OnCandidateRemoved;
            _listener.VotingStarted += OnVotingStarted;
            _listener.VotingEnded += OnVotingEnded;
            _listener.VoteCast += OnVoteCast;
            return _listener.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _listener.StopAsync();
            _listener.SessionCreated -= OnSessionCreated;
            _listener.CandidateAdded -= OnCandidateAdded;
            _listener.CandidateRemoved -= OnCandidateRemoved;
            _listener.VotingStarted -= OnVotingStarted;
            _listener.VotingEnded -= OnVotingEnded;
            _listener.VoteCast -= OnVoteCast;
        }

        private VotingSessionAggregate GetOrCreate(uint sessionId)
            => _sessions.GetOrAdd(sessionId, _ => new VotingSessionAggregate());

        private async void OnSessionCreated(object? _, SessionCreatedEventArgs e)
        {
            // уже сохранённый агрегат с полным off-chain состоянием
            var persisted = await _sessionRepo
                .GetByIdAsync(e.SessionId)
                .ConfigureAwait(false) ?? throw new DomainException($"Session {e.SessionId} not found in repository");

            // применяем on-chain факт к in-memory проекции
            // (id в persisted уже равен e.SessionId, но Apply просто гарантирует consistency)
            GetOrCreate(e.SessionId)
                .Apply(new SessionCreatedDomainEvent(
                    e.SessionId,
                    persisted.AdminUserId,
                    persisted.Mode,
                    persisted.RequiredVerificationLevel
                ));
        }

        private void OnCandidateAdded(object? _, CandidateAddedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new CandidateAddedDomainEvent(e.SessionId, e.CandidateId, e.Name));

        private void OnCandidateRemoved(object? _, CandidateRemovedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new CandidateRemovedDomainEvent(e.SessionId, e.CandidateId));

        private void OnVotingStarted(object? _, VotingStartedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new VotingStartedDomainEvent(e.SessionId, e.StartTimeUtc, e.EndTimeUtc));

        private void OnVotingEnded(object? _, VotingEndedEventArgs e)
            => GetOrCreate(e.SessionId)
               .Apply(new VotingEndedDomainEvent(e.SessionId, e.EndTimeUtc));

        private async void OnVoteCast(object? _, VoteCastEventArgs e)
        {
            var user = await _userRepo
                .GetByBlockchainAddressAsync(e.Voter)
                .ConfigureAwait(false)
                ?? throw new DomainException($"Unknown voter address: {e.Voter}");

            var domEvt = new VoteCastDomainEvent(
                e.SessionId,
                user.Id,
                e.CandidateId
            );

            GetOrCreate(e.SessionId).Apply(domEvt);
        }

        /// <summary> Чтение агрегата из других компонентов. </summary>
        public VotingSessionAggregate? GetAggregate(uint sessionId)
            => _sessions.TryGetValue(sessionId, out var agg) ? agg : null;
    }
}
