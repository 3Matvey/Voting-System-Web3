using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;
using Voting.Domain.Interfaces;
using Voting.Application.Interfaces;
using Voting.Application.Events;
using Voting.Domain.Exceptions;

namespace Voting.Application.Projections
{
    public class VotingSessionProjection : IHostedService
    {
        private readonly IContractEventListener _listener;
        private readonly IUnitOfWork _uow;
        private readonly IDomainEventPublisher _publisher;

        private readonly ConcurrentDictionary<uint, VotingSessionAggregate> _sessions = [];

        // делегаты для контрактных событий
        private readonly EventHandler<SessionCreatedEventArgs> _onChainSessionCreated;
        private readonly EventHandler<CandidateAddedEventArgs> _onChainCandidateAdded;
        private readonly EventHandler<CandidateRemovedEventArgs> _onChainCandidateRemoved;
        private readonly EventHandler<VotingStartedEventArgs> _onChainVotingStarted;
        private readonly EventHandler<VotingEndedEventArgs> _onChainVotingEnded;
        private readonly EventHandler<VoteCastEventArgs> _onChainVoteCast;

        public VotingSessionProjection(
            IContractEventListener listener,
            IUnitOfWork uow,
            IDomainEventPublisher publisher)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

            // on-chain → domain events
            _onChainSessionCreated = (_, e) => _publisher.Publish(new SessionCreatedDomainEvent(
                                            e.SessionId,
                                            Guid.Empty,
                                            default,
                                            default));
            _onChainCandidateAdded = (_, e) => _publisher.Publish(new CandidateAddedDomainEvent(
                                            e.SessionId, e.CandidateId, e.Name));
            _onChainCandidateRemoved = (_, e) => _publisher.Publish(new CandidateRemovedDomainEvent(
                                            e.SessionId, e.CandidateId));
            _onChainVotingStarted = (_, e) => _publisher.Publish(new VotingStartedDomainEvent(
                                            e.SessionId, e.StartTimeUtc, e.EndTimeUtc));
            _onChainVotingEnded = (_, e) => _publisher.Publish(new VotingEndedDomainEvent(
                                            e.SessionId, e.EndTimeUtc));
            _onChainVoteCast = async (_, e) =>
            {
                var user = await _uow.Users
                    .GetByBlockchainAddressAsync(e.Voter)
                    .ConfigureAwait(false)
                    ?? throw new DomainException($"Unknown voter address: {e.Voter}");

                _publisher.Publish(new VoteCastDomainEvent(
                    e.SessionId,
                    user.Id,
                    e.CandidateId
                ));
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // подписываемся на контракта события
            _listener.SessionCreated += _onChainSessionCreated;
            _listener.CandidateAdded += _onChainCandidateAdded;
            _listener.CandidateRemoved += _onChainCandidateRemoved;
            _listener.VotingStarted += _onChainVotingStarted;
            _listener.VotingEnded += _onChainVotingEnded;
            _listener.VoteCast += _onChainVoteCast;

            // подписываемся на доменные события
            _publisher.Subscribe<SessionCreatedDomainEvent>(On);
            _publisher.Subscribe<CandidateAddedDomainEvent>(On);
            _publisher.Subscribe<CandidateRemovedDomainEvent>(On);
            _publisher.Subscribe<VotingStartedDomainEvent>(On);
            _publisher.Subscribe<VotingEndedDomainEvent>(On);
            _publisher.Subscribe<VoteCastDomainEvent>(On);
            _publisher.Subscribe<VoterRegisteredDomainEvent>(On);
            _publisher.Subscribe<CandidateDescriptionUpdatedDomainEvent>(On);

            return _listener.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _listener.StopAsync();

            // отписываемся от контрактных
            _listener.SessionCreated -= _onChainSessionCreated;
            _listener.CandidateAdded -= _onChainCandidateAdded;
            _listener.CandidateRemoved -= _onChainCandidateRemoved;
            _listener.VotingStarted -= _onChainVotingStarted;
            _listener.VotingEnded -= _onChainVotingEnded;
            _listener.VoteCast -= _onChainVoteCast;
        }

        private VotingSessionAggregate GetOrCreate(uint sessionId)
            => _sessions.GetOrAdd(sessionId, _ => new VotingSessionAggregate());

        // общий метод-обработчик всех доменных событий
        private async void On(SessionCreatedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async void On(CandidateAddedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async void On(CandidateRemovedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async void On(VotingStartedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async void On(VotingEndedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async void On(VoteCastDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async void On(VoterRegisteredDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async void On(CandidateDescriptionUpdatedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg);
        }

        private async Task UpsertAsync(VotingSessionAggregate agg)
        {
            var existing = await _uow.VotingSessions.GetByIdAsync(agg.Id).ConfigureAwait(false);
            if (existing == null)
                await _uow.VotingSessions.AddAsync(agg).ConfigureAwait(false);
            else
                await _uow.VotingSessions.UpdateAsync(agg).ConfigureAwait(false);

            await _uow.CommitAsync().ConfigureAwait(false);
        }

        public VotingSessionAggregate? GetAggregate(uint sessionId)
            => _sessions.TryGetValue(sessionId, out var agg) ? agg : null;
    }
}
