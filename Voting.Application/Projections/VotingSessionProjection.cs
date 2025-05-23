using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;
using Voting.Domain.Exceptions;
using Voting.Domain.Interfaces;
using Voting.Application.Events;
using Voting.Application.Interfaces;

namespace Voting.Application.Projections
{
    public class VotingSessionProjection : IHostedService
    {
        private readonly IContractEventListener _listener;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDomainEventPublisher _publisher;

        // кеш агрегатов
        private readonly ConcurrentDictionary<uint, VotingSessionAggregate> _sessions = new();

        // делегаты для контрактных событий
        //private readonly EventHandler<SessionCreatedEventArgs> _onChainSessionCreated;
        private readonly EventHandler<CandidateAddedEventArgs> _onChainCandidateAdded;
        private readonly EventHandler<CandidateRemovedEventArgs> _onChainCandidateRemoved;
        private readonly EventHandler<VotingStartedEventArgs> _onChainVotingStarted;
        private readonly EventHandler<VotingEndedEventArgs> _onChainVotingEnded;
        private readonly EventHandler<VoteCastEventArgs> _onChainVoteCast;

        public VotingSessionProjection(
            IContractEventListener listener,
            IServiceScopeFactory scopeFactory,
            IDomainEventPublisher publisher)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

            // on-chain → domain events
            //_onChainSessionCreated = (_, e) =>
            //    _publisher.Publish(new SessionCreatedDomainEvent(
            //        e.SessionId, Guid.Empty, default, default));

            _onChainCandidateAdded = (_, e) =>
                _publisher.Publish(new CandidateAddedDomainEvent(
                    e.SessionId, e.CandidateId, e.Name));

            _onChainCandidateRemoved = (_, e) =>
                _publisher.Publish(new CandidateRemovedDomainEvent(
                    e.SessionId, e.CandidateId));

            _onChainVotingStarted = (_, e) =>
                _publisher.Publish(new VotingStartedDomainEvent(
                    e.SessionId, e.StartTimeUtc, e.EndTimeUtc));

            _onChainVotingEnded = (_, e) =>
                _publisher.Publish(new VotingEndedDomainEvent(
                    e.SessionId, e.EndTimeUtc));

            // для VoteCast создаём scope для получения IUnitOfWork
            _onChainVoteCast = async (_, e) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var user = await uow.Users
                    .GetByBlockchainAddressAsync(e.Voter)
                    .ConfigureAwait(false)
                    ?? throw new DomainException($"Unknown voter address: {e.Voter}");

                _publisher.Publish(new VoteCastDomainEvent(
                    e.SessionId, user.Id, e.CandidateId));
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // подписываемся на on-chain события
            //_listener.SessionCreated += _onChainSessionCreated;
            _listener.CandidateAdded += _onChainCandidateAdded;
            _listener.CandidateRemoved += _onChainCandidateRemoved;
            _listener.VotingStarted += _onChainVotingStarted;
            _listener.VotingEnded += _onChainVotingEnded;
            _listener.VoteCast += _onChainVoteCast;

            // подписываемся на доменные события
            _publisher.Subscribe<SessionCreatedDomainEvent>(OnSessionCreated);
            _publisher.Subscribe<CandidateAddedDomainEvent>(OnCandidateAdded);
            _publisher.Subscribe<CandidateRemovedDomainEvent>(OnCandidateRemoved);
            _publisher.Subscribe<VotingStartedDomainEvent>(OnVotingStarted);
            _publisher.Subscribe<VotingEndedDomainEvent>(OnVotingEnded);
            _publisher.Subscribe<VoteCastDomainEvent>(OnVoteCast);
            _publisher.Subscribe<VoterRegisteredDomainEvent>(OnVoterRegistered);
            _publisher.Subscribe<CandidateDescriptionUpdatedDomainEvent>(OnCandidateDescriptionUpdated);

            return _listener.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _listener.StopAsync().ConfigureAwait(false);

            //_listener.SessionCreated -= _onChainSessionCreated;
            _listener.CandidateAdded -= _onChainCandidateAdded;
            _listener.CandidateRemoved -= _onChainCandidateRemoved;
            _listener.VotingStarted -= _onChainVotingStarted;
            _listener.VotingEnded -= _onChainVotingEnded;
            _listener.VoteCast -= _onChainVoteCast;
        }

        private VotingSessionAggregate GetOrCreate(uint sessionId)
            => _sessions.GetOrAdd(sessionId, _ => new VotingSessionAggregate());

        private async void OnSessionCreated(SessionCreatedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async void OnCandidateAdded(CandidateAddedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async void OnCandidateRemoved(CandidateRemovedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async void OnVotingStarted(VotingStartedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async void OnVotingEnded(VotingEndedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async void OnVoteCast(VoteCastDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async void OnVoterRegistered(VoterRegisteredDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async void OnCandidateDescriptionUpdated(CandidateDescriptionUpdatedDomainEvent e)
        {
            var agg = GetOrCreate(e.SessionId).Apply(e);
            await UpsertAsync(agg).ConfigureAwait(false);
        }

        private async Task UpsertAsync(VotingSessionAggregate agg)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var existing = await uow.VotingSessions.GetByIdAsync(agg.Id).ConfigureAwait(false);
            if (existing == null)
                await uow.VotingSessions.AddAsync(agg).ConfigureAwait(false);
            else 
            {
                existing = agg; //FIXME надо подумать зачем оно
                await uow.VotingSessions.UpdateAsync(agg).ConfigureAwait(false);
            }

            await uow.CommitAsync().ConfigureAwait(false);
        }

        public VotingSessionAggregate? GetAggregate(uint sessionId)
            => _sessions.TryGetValue(sessionId, out var agg) ? agg : null;
    }
}
