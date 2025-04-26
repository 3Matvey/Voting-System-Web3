using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Voting.Application.Interfaces;
using Voting.Application.Events;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;
using Voting.Domain.Interfaces.Repositories;
using Voting.Domain.Exceptions;

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

        private void OnSessionCreated(object? _, SessionCreatedEventArgs e)
        {
            // off-chain конфиг — режим
            var mode = _sessionRepo
                .GetRegistrationModeAsync(e.SessionId)
                .GetAwaiter().GetResult()
                ?? throw new InvalidOperationException($"Mode для sessionId={e.SessionId} не найден");

            // маппинг on-chain address → userId
            var admin = _userRepo
                .GetByBlockchainAddressAsync(e.SessionAdmin)
                .GetAwaiter().GetResult()
                ?? throw new DomainException($"Unknown admin address: {e.SessionAdmin}");

            var domEvt = new SessionCreatedDomainEvent(
                e.SessionId,
                admin.Id,
                mode
            );
            GetOrCreate(e.SessionId).Apply(domEvt);
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

        private void OnVoteCast(object? _, VoteCastEventArgs e)
        {
            // маппинг адрес → Guid
            var user = _userRepo
                .GetByBlockchainAddressAsync(e.Voter)
                .GetAwaiter().GetResult()
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
