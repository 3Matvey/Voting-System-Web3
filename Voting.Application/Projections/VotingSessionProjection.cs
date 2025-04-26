using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Voting.Application.Interfaces;
using Voting.Domain.Aggregates;
using Voting.Domain.Events;
using Voting.Application.Events;

namespace Voting.Application.Projections
{
    /// <summary>
    /// Проекция сессий голосования.
    /// Хранит агрегаты VotingSessionAggregate в памяти и обновляет их при событиях контракта.
    /// </summary>
    public class VotingSessionProjection(IContractEventListener listener) 
        : IHostedService
    {
        private readonly IContractEventListener _listener = listener;
        private readonly ConcurrentDictionary<uint, VotingSessionAggregate> _sessions
            = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Подписываемся на события контракта
            _listener.SessionCreated += OnSessionCreated;
            _listener.CandidateAdded += OnCandidateAdded;
            _listener.CandidateRemoved += OnCandidateRemoved;
            _listener.VotingStarted += OnVotingStarted;
            _listener.VotingEnded += OnVotingEnded;
            _listener.VoteCast += OnVoteCast;

            // Запускаем слушатель (запросит past+future события)
            return _listener.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _listener.StopAsync();

            // Отписаться, чтобы можно было пересоздать Start/Stop
            _listener.SessionCreated -= OnSessionCreated;
            _listener.CandidateAdded -= OnCandidateAdded;
            _listener.CandidateRemoved -= OnCandidateRemoved;
            _listener.VotingStarted -= OnVotingStarted;
            _listener.VotingEnded -= OnVotingEnded;
            _listener.VoteCast -= OnVoteCast;
        }

        private VotingSessionAggregate GetOrCreate(uint sessionId)
        {
            return _sessions.GetOrAdd(sessionId, id => new VotingSessionAggregate(id)); //тут пока ошибка компиляции
        }

        private void OnSessionCreated(object? sender, SessionCreatedEventArgs e)
        {
            var agg = GetOrCreate(e.SessionId);
            agg.Apply(new SessionCreatedDomainEvent(e.SessionId, e.SessionAdmin));
        }

        private void OnCandidateAdded(object? sender, CandidateAddedEventArgs e)
        {
            var agg = GetOrCreate(e.SessionId);
            agg.Apply(new CandidateAddedDomainEvent(e.SessionId, e.CandidateId, e.Name));
        }

        private void OnCandidateRemoved(object? sender, CandidateRemovedEventArgs e)
        {
            var agg = GetOrCreate(e.SessionId);
            agg.Apply(new CandidateRemovedDomainEvent(e.SessionId, e.CandidateId));
        }

        private void OnVotingStarted(object? sender, VotingStartedEventArgs e)
        {
            var agg = GetOrCreate(e.SessionId);
            agg.Apply(new VotingStartedDomainEvent(e.SessionId, e.StartTimeUtc, e.EndTimeUtc));
        }

        private void OnVotingEnded(object? sender, VotingEndedEventArgs e)
        {
            var agg = GetOrCreate(e.SessionId);
            agg.Apply(new VotingEndedDomainEvent(e.SessionId, e.EndTimeUtc));
        }

        private void OnVoteCast(object? sender, VoteCastEventArgs e)
        {
            var agg = GetOrCreate(e.SessionId);
            agg.Apply(new VoteCastDomainEvent(e.SessionId, e.Voter, e.CandidateId));
        }

        /// <summary>
        /// Позволяет другим сервисам получать агрегат по идентификатору.
        /// </summary>
        public VotingSessionAggregate? GetAggregate(uint sessionId)
            => _sessions.TryGetValue(sessionId, out var agg) ? agg : null;
    }
}
