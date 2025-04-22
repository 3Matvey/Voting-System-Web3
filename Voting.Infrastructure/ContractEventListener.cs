using Microsoft.Extensions.Logging;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Voting.Infrastructure.Blockchain.EventDTOs;
using Voting.Application.Events;
using Voting.Application.Interfaces;

namespace Voting.Infrastructure.Blockchain
{
    /// <summary>
    /// Слушатель событий смарт-контракта через WebSocket (Streaming).
    /// Подписывается на все основные события и вызывает заданные обработчики.
    /// </summary>
    public class ContractEventListener(string wsUrl, string contractAddress, ILogger<ContractEventListener>? logger = null)
        : IContractEventListener
    {
        private readonly StreamingWebSocketClient _client = new(wsUrl);

        private readonly List<EthLogsObservableSubscription> _subscriptions = new();
        private readonly List<IDisposable> _disposables = new();

        private bool _started;
        private bool _disposed;

        public event EventHandler<SessionCreatedEventArgs>? SessionCreated;
        public event EventHandler<CandidateAddedEventArgs>? CandidateAdded;
        public event EventHandler<CandidateRemovedEventArgs>? CandidateRemoved;
        public event EventHandler<VotingStartedEventArgs>? VotingStarted;
        public event EventHandler<VotingEndedEventArgs>? VotingEnded;
        public event EventHandler<VoteCastEventArgs>? VoteCast;

        public async Task StartAsync()
        {
            if (_started) return;
            _started = true;
            await _client.StartAsync();
            logger?.LogInformation("WebSocket connection started to {Url}", wsUrl);
            await InitializeSubscriptionsAsync();
        }

        public async Task StopAsync()
        {
            _started = false;

            foreach (var sub in _subscriptions) 
                await sub.UnsubscribeAsync();
            _subscriptions.Clear();

            foreach (var disp in _disposables)
                disp.Dispose();
            _disposables.Clear();

            await _client.StopAsync();
            logger?.LogInformation("WebSocket connection stopped");
        }

        private async Task InitializeSubscriptionsAsync()
        {
            await Subscribe<SessionCreatedEventDTO, SessionCreatedEventArgs>(
                dto => new SessionCreatedEventArgs(
                    sessionId: checked((uint)dto.SessionId),
                    sessionAdmin: dto.SessionAdmin),
                (s, e) => SessionCreated?.Invoke(s, e)
            );

            await Subscribe<CandidateAddedEventDTO, CandidateAddedEventArgs>(
                dto => new CandidateAddedEventArgs(
                    sessionId: checked((uint)dto.SessionId),
                    candidateId: checked((uint)dto.CandidateId),
                    name: dto.Name),
                (s, e) => CandidateAdded?.Invoke(s, e)
            );

            await Subscribe<CandidateRemovedEventDTO, CandidateRemovedEventArgs>(
                dto => new CandidateRemovedEventArgs(
                    sessionId: checked((uint)dto.SessionId),
                    candidateId: checked((uint)dto.CandidateId),
                    name: dto.Name),
                (s, e) => CandidateRemoved?.Invoke(s, e)
            );

            await Subscribe<VotingStartedEventDTO, VotingStartedEventArgs>(
                dto => new VotingStartedEventArgs(
                    sessionId: checked((uint)dto.SessionId),
                    startTimeUtc: DateTimeOffset.FromUnixTimeSeconds((long)dto.StartTime).UtcDateTime,
                    endTimeUtc: DateTimeOffset.FromUnixTimeSeconds((long)dto.EndTime).UtcDateTime),
                (s, e) => VotingStarted?.Invoke(s, e)
            );

            await Subscribe<VotingEndedEventDTO, VotingEndedEventArgs>(
                dto => new VotingEndedEventArgs(
                    sessionId: checked((uint)dto.SessionId),
                    endTimeUtc: DateTimeOffset.FromUnixTimeSeconds((long)dto.EndTime).UtcDateTime),
                (s, e) => VotingEnded?.Invoke(s, e)
            );

            await Subscribe<VoteCastEventDTO, VoteCastEventArgs>(
                dto => new VoteCastEventArgs(
                    sessionId: checked((uint)dto.SessionId),
                    voter: dto.Voter,
                    candidateId: checked((uint)dto.CandidateId)),
                (s, e) => VoteCast?.Invoke(s, e)
            );
        }

        private async Task Subscribe<TDto, TArgs>(Func<TDto, TArgs> map, Action<object, TArgs> raise)
            where TDto : class, IEventDTO, new()
            where TArgs : EventArgs
        {
            var subscription = new EthLogsObservableSubscription(_client); // обёртка подписки на логи Ethereum через WebSocket
            var disposable = subscription
                .GetSubscriptionDataResponsesAsObservable() // получает IObservable<Log> — последовательность входящих лог‑сообщений
                .Subscribe(log =>                           // подписывается на неё через Rx (подписка на каждый лог)
                {
                    try
                    {
                        var decoded = Event<TDto>.DecodeEvent(log)?.Event;
                        if (decoded is null) return;
                        var args = map(decoded);
                        raise(this, args);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Error handling {Event}", typeof(TDto).Name);
                    }
                },
                err => logger?.LogError(err, "Subscription error for {Event}", typeof(TDto).Name));

            _subscriptions.Add(subscription);
            _disposables.Add(disposable);

            var filter = Event<TDto>
                .GetEventABI()
                .CreateFilterInput(contractAddress);

            await subscription.SubscribeAsync(filter);  // Запуск подписки: WebSocket становится «управляемым» слушателем
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            await StopAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
