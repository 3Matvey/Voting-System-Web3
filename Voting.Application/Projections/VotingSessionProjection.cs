using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Voting.Application.Interfaces;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;
using Voting.Domain.Exceptions;
using Voting.Domain.Interfaces;

namespace Voting.Application.Projections
{
    /// <summary>
    /// Проекция, которая последовательно обрабатывает доменные события и обновляет БД.
    /// </summary>
    public class VotingSessionProjection : IHostedService
    {
        private readonly IContractEventListener _listener;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDomainEventPublisher _publisher;

        // очередь доменных событий
        private readonly Channel<EventEnvelope> _channel = Channel.CreateUnbounded<EventEnvelope>();

        // кеш агрегатов в памяти
        private readonly ConcurrentDictionary<uint, VotingSessionAggregate> _sessions = new();

        // пер-сессия семафоры для синхронизации записи
        private static readonly ConcurrentDictionary<uint, SemaphoreSlim> _sessionLocks = new();

        public VotingSessionProjection(
            IContractEventListener listener,
            IServiceScopeFactory scopeFactory,
            IDomainEventPublisher publisher)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // on-chain → публикация доменных событий
            _listener.CandidateAdded += (_, e) => _publisher.Publish(new CandidateAddedDomainEvent(e.SessionId, e.CandidateId, e.Name));
            _listener.CandidateRemoved += (_, e) => _publisher.Publish(new CandidateRemovedDomainEvent(e.SessionId, e.CandidateId));
            _listener.VotingStarted += (_, e) => _publisher.Publish(new VotingStartedDomainEvent(e.SessionId, e.StartTimeUtc, e.EndTimeUtc));
            _listener.VotingEnded += (_, e) => _publisher.Publish(new VotingEndedDomainEvent(e.SessionId, e.EndTimeUtc));
            _listener.VoteCast += async (_, e) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await uow.Users.GetByBlockchainAddressAsync(e.Voter)
                    .ConfigureAwait(false)
                    ?? throw new DomainException($"Unknown voter address: {e.Voter}");
                _publisher.Publish(new VoteCastDomainEvent(e.SessionId, user.Id, e.CandidateId));
            };

            // подписываем доменные события на очередь
            _publisher.Subscribe<SessionCreatedDomainEvent>(e => Enqueue(e.SessionId, e));
            _publisher.Subscribe<CandidateAddedDomainEvent>(e => Enqueue(e.SessionId, e));
            _publisher.Subscribe<CandidateRemovedDomainEvent>(e => Enqueue(e.SessionId, e));
            _publisher.Subscribe<VotingStartedDomainEvent>(e => Enqueue(e.SessionId, e));
            _publisher.Subscribe<VotingEndedDomainEvent>(e => Enqueue(e.SessionId, e));
            _publisher.Subscribe<VoteCastDomainEvent>(e => Enqueue(e.SessionId, e));
            _publisher.Subscribe<VoterRegisteredDomainEvent>(e => Enqueue(e.SessionId, e));
            _publisher.Subscribe<CandidateDescriptionUpdatedDomainEvent>(e => Enqueue(e.SessionId, e));

            // запускаем фоновую задачу для последовательной обработки
            _ = Task.Run(() => ProcessLoop(cancellationToken), cancellationToken);

            return _listener.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _listener.StopAsync().ConfigureAwait(false);
            _channel.Writer.Complete();
        }

        private void Enqueue(uint sessionId, object @event)
        {
            _channel.Writer.TryWrite(new EventEnvelope(sessionId, @event));
        }

        private async Task ProcessLoop(CancellationToken cancellationToken)
        {
            var reader = _channel.Reader;
            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (reader.TryRead(out var envelope))
                {
                    await ProcessEvent(envelope).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessEvent(EventEnvelope envelope)
        {
            var sem = _sessionLocks.GetOrAdd(envelope.SessionId, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync().ConfigureAwait(false);
            try
            {
                var agg = await GetOrCreateAsync(envelope.SessionId).ConfigureAwait(false);

                switch (envelope.Event)
                {
                    case SessionCreatedDomainEvent e:
                        agg.Apply(e);
                        break;
                    case CandidateAddedDomainEvent e:
                        agg.Apply(e);
                        break;
                    case CandidateRemovedDomainEvent e:
                        agg.Apply(e);
                        break;
                    case VotingStartedDomainEvent e:
                        agg.Apply(e);
                        break;
                    case VotingEndedDomainEvent e:
                        agg.Apply(e);
                        break;
                    case VoteCastDomainEvent e:
                        agg.Apply(e);
                        break;
                    case VoterRegisteredDomainEvent e:
                        agg.Apply(e);
                        break;
                    case CandidateDescriptionUpdatedDomainEvent e:
                        agg.Apply(e);
                        break;
                    default:
                        throw new InvalidOperationException($"Unhandled event type: {envelope.Event.GetType().Name}");
                }

                await UpsertAsync(agg).ConfigureAwait(false);
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task<VotingSessionAggregate> GetOrCreateAsync(uint sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var existing))
                return existing;

            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var fromDb = await uow.VotingSessions.GetByIdAsync(sessionId).ConfigureAwait(false);

            var agg = fromDb ?? new VotingSessionAggregate();
            _sessions.TryAdd(sessionId, agg);
            return agg;
        }

        private async Task UpsertAsync(VotingSessionAggregate agg)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var existing = await uow.VotingSessions.GetByIdAsync(agg.Id).ConfigureAwait(false);
            if (existing == null)
                await uow.VotingSessions.AddAsync(agg).ConfigureAwait(false);
            else
                await uow.VotingSessions.UpdateAsync(agg).ConfigureAwait(false);

            await uow.CommitAsync().ConfigureAwait(false);
        }

        public VotingSessionAggregate? GetAggregate(uint sessionId)
            => _sessions.TryGetValue(sessionId, out var agg) ? agg : null;

        /// <summary>
        /// Обёртка для подачи событий в очередь
        /// </summary>
        private readonly record struct EventEnvelope(uint SessionId, object Event);
    }
}
